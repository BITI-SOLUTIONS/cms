// ================================================================================
// ARCHIVO: CMS.UI/Program.cs
// PROPÓSITO: Configuración y arranque de la interfaz web del Sistema CMS
// DESCRIPCIÓN: BOOTSTRAP desde connectionstrings.json + Configuración desde BD
//              Lee [ADMIN].[COMPANY] para obtener TODA la configuración
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ACTUALIZADO: 2026-02-10
// ================================================================================

using CMS.UI;
using CMS.UI.Services;
using CMS.Data;
using CMS.Data.Services;
using CMS.Entities;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ⭐ Configuración de Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 2;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ================================================================================
// FASE 1: DETECTAR AMBIENTE
// ================================================================================
var isRunningInDocker = File.Exists("/.dockerenv");
var sharedConfigPath = isRunningInDocker
    ? "/app/connectionstrings.json"
    : Path.Combine(builder.Environment.ContentRootPath, "..", "CMS.API", "connectionstrings.json");

sharedConfigPath = Path.GetFullPath(sharedConfigPath);

if (!File.Exists(sharedConfigPath))
{
    throw new FileNotFoundException($"❌ No se encontró: {sharedConfigPath}");
}

Console.WriteLine($"✅ Cargando desde: {sharedConfigPath}");

builder.Configuration.AddJsonFile(sharedConfigPath, optional: false, reloadOnChange: true);

// ⭐ LEER AMBIENTE
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? builder.Configuration["Environment"]
    ?? "Development";

var isDevelopment = environment == "Development";

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  🌍 Ambiente: {environment.PadRight(45)}║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

// ⭐ LEER CONFIGURACIÓN
var companySchema = builder.Configuration["CompanySchema"]
    ?? throw new InvalidOperationException("❌ 'CompanySchema' no configurado");

var bootstrapConnectionString = builder.Configuration[$"ConnectionStrings:{environment}:DefaultConnection"]
    ?? throw new InvalidOperationException($"❌ ConnectionStrings:{environment}:DefaultConnection no encontrado");

var apiBaseUrlFromConfig = builder.Configuration[$"ApiSettings:{environment}:BaseUrl"];

Console.WriteLine($"📂 Schema: {companySchema}");
Console.WriteLine($"🗄️  BD: {(isDevelopment ? "10.0.0.1 (Development)" : "cms-postgres (Production)")}");
Console.WriteLine($"🔗 API: {apiBaseUrlFromConfig}");

// ================================================================================
// FASE 2: CONFIGURAR DbContext
// ================================================================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(bootstrapConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(60);
    });

    if (isDevelopment)
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CompanyConfigService>();

// ================================================================================
// FASE 3: CARGAR CONFIGURACIÓN DESDE BD
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
    throw new InvalidOperationException($"❌ Error cargando configuración: {ex.Message}", ex);
}

Console.WriteLine($"✅ Compañía: {companyConfig.COMPANY_NAME}");

var apiBaseUrl = apiBaseUrlFromConfig ?? companyConfig.GetAPIBaseUrl();
var environmentName = isDevelopment ? "DEVELOPMENT" : "PRODUCTION";

builder.Services.AddSingleton(companyConfig);

// ================================================================================
// SERVICIOS
// ================================================================================

// 1. AUTENTICACIÓN
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
            ?? new[] { "api://b231a44d-7e9d-4d9b-8866-9a4b3c5ab5cd/access_as_user" };

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
                    logger.LogWarning("⚠️ No se pudo sincronizar usuario");
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

// 2. MVC
builder.Services.AddControllersWithViews();

// 3. HTTP CLIENTS
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

// 4. SERVICIOS PERSONALIZADOS
builder.Services.AddScoped<MenuApiService>();
builder.Services.AddScoped<UserSyncApiService>();
builder.Services.AddScoped<SettingsApiService>();
builder.Services.AddTransient<AuthenticatedApiMessageHandler>();

// ================================================================================
// PIPELINE
// ================================================================================
var app = builder.Build();

app.UseForwardedHeaders();

if (isDevelopment)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  ✅ CMS.UI INICIADA - {environmentName.PadRight(36)}║");
Console.WriteLine($"║  🌐 URLs: {string.Join(", ", app.Urls).PadRight(46)}║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

app.Run();