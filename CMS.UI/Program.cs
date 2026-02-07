// ================================================================================
// ARCHIVO: CMS.UI/Program.cs
// PROPÓSITO: Configuración y arranque de la interfaz web del Sistema CMS
// DESCRIPCIÓN: BOOTSTRAP desde connectionstrings.json + Configuración desde BD
//              Lee [ADMIN].[COMPANY] para obtener TODA la configuración
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ACTUALIZADO: 2026-02-07
// ================================================================================

using CMS.UI;
using CMS.UI.Services;
using CMS.Data;
using CMS.Data.Services;
using CMS.Entities;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;  // ⭐ AGREGAR ESTO
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ⭐ AGREGAR ESTA SECCIÓN - Configuración de Forwarded Headers para Traefik
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 2;
    // Permitir desde cualquier proxy (seguro porque estamos en Kubernetes)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ================================================================================
// FASE 1: BOOTSTRAP - Cargar configuración mínima desde connectionstrings.json
// ================================================================================
// ⭐ OBJETIVO: Obtener SOLO la cadena de conexión para consultar la BD
// ⭐ TODO LO DEMÁS se carga desde [ADMIN].[COMPANY]

// Try to load from local CMS.UI directory first, then fallback to shared locations
var localConfigPath = Path.Combine(builder.Environment.ContentRootPath, "connectionstrings.json");
var sharedConfigPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "..",
    "CMS.API",
    "connectionstrings.json"
);
var dockerConfigPath = "/app/connectionstrings.json";

string configPath;
if (File.Exists(localConfigPath))
{
    configPath = localConfigPath;
}
else if (File.Exists(dockerConfigPath))
{
    configPath = dockerConfigPath;
}
else if (File.Exists(sharedConfigPath))
{
    configPath = Path.GetFullPath(sharedConfigPath);
}
else
{
    configPath = localConfigPath; // Use for error message
}

if (!File.Exists(configPath))
{
    throw new FileNotFoundException(
        $"❌ ERROR: No se encontró 'connectionstrings.json' en ninguna de las ubicaciones:\n" +
        $"   1. {localConfigPath}\n" +
        $"   2. {dockerConfigPath}\n" +
        $"   3. {sharedConfigPath}\n\n" +
        "   Este archivo debe contener:\n" +
        "   {\n" +
        "     \"CompanySchema\": \"admin\",\n" +
        "     \"ConnectionString\": \"Host=...;Database=cms;...\"\n" +
        "   }\n\n" +
        "   La configuración completa se carga desde admin.company"
    );
}

Console.WriteLine($"✅ Cargando bootstrap desde: {configPath}");

builder.Configuration.AddJsonFile(configPath, optional: false, reloadOnChange: true);

// Leer parámetros bootstrap
var companySchema = builder.Configuration.GetValue<string>("CompanySchema")
    ?? throw new InvalidOperationException("❌ 'CompanySchema' no está configurado");

var bootstrapConnectionString = builder.Configuration.GetValue<string>("ConnectionString")
    ?? throw new InvalidOperationException("❌ 'ConnectionString' no está configurado");

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  🔄 BOOTSTRAP - Compañía: {companySchema,-34} ║");
Console.WriteLine($"║  📊 Conectando a BD para cargar configuración...            ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

// ================================================================================
// FASE 2: CONFIGURAR DbContext CON CADENA BOOTSTRAP (PostgreSQL)
// ================================================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(bootstrapConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(60);
    })
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CompanyConfigService>();

// ================================================================================
// FASE 3: CARGAR CONFIGURACIÓN DESDE [ADMIN].[COMPANY]
// ================================================================================

var serviceProvider = builder.Services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var configService = scope.ServiceProvider.GetRequiredService<CompanyConfigService>();

Company companyConfig;
try
{
    companyConfig = await configService.GetCompanyConfigAsync(companySchema);
}
catch (Exception ex)
{
    throw new InvalidOperationException(
        $"❌ ERROR: No se pudo cargar configuración de '{companySchema}' desde admin.company\n" +
        $"   Verifica que:\n" +
        $"   1. La tabla admin.company existe\n" +
        $"   2. Existe un registro con company_schema = '{companySchema}'\n" +
        $"   3. is_active = true\n\n" +
        $"   Error: {ex.Message}",
        ex
    );
}

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  ✅ Configuración cargada desde admin.company               ║");
Console.WriteLine($"║  🏢 Compañía: {companyConfig.COMPANY_NAME.PadRight(44)}║");
Console.WriteLine($"║  📂 Schema: {companyConfig.COMPANY_SCHEMA.PadRight(48)}║");
Console.WriteLine($"║  🎯 Entorno: {(companyConfig.IS_PRODUCTION ? "PRODUCTION" : "DEVELOPMENT").PadRight(47)}║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

var apiBaseUrl = companyConfig.GetAPIBaseUrl();
var environmentName = companyConfig.IS_PRODUCTION ? "PRODUCTION" : "DEVELOPMENT";

builder.Services.AddSingleton(companyConfig);

// ================================================================================
// CONFIGURACIÓN DE SERVICIOS
// ================================================================================

// -----------------------------------------------
// 1. AUTENTICACIÓN CON AZURE AD - DESDE BD
// -----------------------------------------------
var azureAdConfig = new Dictionary<string, string>
{
    ["AzureAd:Instance"] = companyConfig.AZURE_AD_UI_INSTANCE ?? throw new InvalidOperationException("AZURE_AD_UI_INSTANCE no configurado"),
    ["AzureAd:TenantId"] = companyConfig.AZURE_AD_UI_TENANT_ID ?? throw new InvalidOperationException("AZURE_AD_UI_TENANT_ID no configurado"),
    ["AzureAd:Domain"] = companyConfig.AZURE_AD_UI_DOMAIN ?? throw new InvalidOperationException("AZURE_AD_UI_DOMAIN no configurado"),
    ["AzureAd:ClientId"] = companyConfig.AZURE_AD_UI_CLIENT_ID ?? throw new InvalidOperationException("AZURE_AD_UI_CLIENT_ID no configurado"),
    ["AzureAd:ClientSecret"] = companyConfig.AZURE_AD_UI_CLIENT_SECRET ?? throw new InvalidOperationException("AZURE_AD_UI_CLIENT_SECRET no configurado"),
    ["AzureAd:CallbackPath"] = companyConfig.AZURE_AD_UI_CALL_BACK_PATH ?? "/signin-oidc"
};

var inMemoryConfig = new ConfigurationBuilder()
    .AddInMemoryCollection(azureAdConfig!)
    .Build();

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        inMemoryConfig.GetSection("AzureAd").Bind(options);

        var scopes = companyConfig.API_SCOPES?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? new[] { "api://8fc7045f-dadd-4de8-892e-9ff446d7f526/access_as_user" };

        foreach (var scope in scopes)
        {
            options.Scope.Add(scope);
        }

        options.ResponseType = "code";
        options.SaveTokens = true;

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                var services = context.HttpContext.RequestServices;
                var syncService = services.GetRequiredService<UserSyncApiService>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                var principal = context.Principal;
                if (principal == null) return;

                var azureUser = new UserSyncApiService.AzureUserInfo
                {
                    ObjectId = principal.FindFirstValue("oid"),
                    UserPrincipalName = principal.FindFirstValue("preferred_username") ?? principal.FindFirstValue(ClaimTypes.Upn),
                    DisplayName = principal.FindFirstValue("name") ?? principal.Identity?.Name,
                    Email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("preferred_username")
                };

                var result = await syncService.SyncAzureUserAsync(azureUser);

                if (result == null)
                {
                    logger.LogWarning("⚠️ No se pudo sincronizar el usuario con CMS.API");
                }
                else
                {
                    logger.LogInformation("✅ Usuario sincronizado: {User}", result.Username);
                }
            }
        };
    })
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// -----------------------------------------------
// 2. MVC / RAZOR PAGES
// -----------------------------------------------
builder.Services.AddControllersWithViews();

// -----------------------------------------------
// 3. HTTP CLIENTS - DESDE BD
// -----------------------------------------------
builder.Services.AddHttpClient("cmsapi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddHttpClient("cmsapi-authenticated", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
})
.AddHttpMessageHandler<AuthenticatedApiMessageHandler>();

// -----------------------------------------------
// 4. SERVICIOS PERSONALIZADOS
// -----------------------------------------------
builder.Services.AddScoped<MenuApiService>();
builder.Services.AddScoped<UserSyncApiService>();
builder.Services.AddScoped<SettingsApiService>();
builder.Services.AddTransient<AuthenticatedApiMessageHandler>();

// ================================================================================
// CONFIGURACIÓN DEL PIPELINE
// ================================================================================

var app = builder.Build();

// ⭐ AGREGAR ESTO - Middleware de Forwarded Headers (DEBE SER UNO DE LOS PRIMEROS)
app.UseForwardedHeaders();

if (!companyConfig.IS_PRODUCTION)
{
    app.UseDeveloperExceptionPage();
    Console.WriteLine("🛠️  Modo desarrollo: Excepciones detalladas habilitadas");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    Console.WriteLine("🔒 Modo producción: Manejo seguro de errores habilitado");
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  ✅ CMS.UI INICIADA - {environmentName,-34} ║");
Console.WriteLine($"║  🌐 URLs: {string.Join(", ", app.Urls).PadRight(46)}║");
Console.WriteLine($"║  🔗 API: {apiBaseUrl.PadRight(50)}║");
Console.WriteLine("╚═════════════════════════════════════════════════���════════════╝");

app.Run();