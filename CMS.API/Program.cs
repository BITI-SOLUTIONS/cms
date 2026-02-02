// ================================================================================
// ARCHIVO: CMS.API/Program.cs
// PROPÓSITO: Configuración y arranque de la API REST del Sistema CMS
// DESCRIPCIÓN: BOOTSTRAP desde connectionstrings.json + Configuración desde BD
//              Lee [ADMIN].[COMPANY] para obtener TODA la configuración
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ACTUALIZADO: 2025-12-19
// ================================================================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using CMS.API.Middleware;
using CMS.Data;
using CMS.Data.Services;
using CMS.Entities;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ================================================================================
// FASE 1: BOOTSTRAP - Cargar configuración mínima desde connectionstrings.json
// ================================================================================

var connectionConfigPath = Path.Combine(builder.Environment.ContentRootPath, "connectionstrings.json");

if (!File.Exists(connectionConfigPath))
{
    throw new FileNotFoundException(
        $"❌ ERROR: No se encontró 'connectionstrings.json' en: {connectionConfigPath}\n" +
        "   Este archivo debe contener:\n" +
        "   {\n" +
        "     \"CompanySchema\": \"admin\",\n" +
        "     \"ConnectionString\": \"Host=...;Database=bi_cccn;...\"\n" +
        "   }"
    );
}

Console.WriteLine($"✅ Cargando bootstrap desde: {connectionConfigPath}");

builder.Configuration.AddJsonFile("connectionstrings.json", optional: false, reloadOnChange: true);

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
builder.Services.AddScoped<PermissionService>();

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

var connectionString = companyConfig.GetConnectionString();
var environmentName = companyConfig.IS_PRODUCTION ? "PRODUCTION" : "DEVELOPMENT";

builder.Services.AddSingleton(companyConfig);

// ================================================================================
// FASE 4: RECONFIGURAR DbContext CON CADENA CORRECTA DESDE BD (PostgreSQL)
// ================================================================================

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(60);
    });

    if (!companyConfig.IS_PRODUCTION)
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}, ServiceLifetime.Scoped);

// ================================================================================
// CONFIGURACIÓN DE SERVICIOS
// ================================================================================

// -----------------------------------------------
// 1. AUTENTICACIÓN (JWT con Azure AD) - DESDE BD
// -----------------------------------------------
// ⭐ CORRECCIÓN: Crear configuración in-memory para Azure AD
var azureAdConfig = new Dictionary<string, string>
{
    ["AzureAd:Instance"] = companyConfig.AZURE_AD_API_INSTANCE ?? throw new InvalidOperationException("AZURE_AD_API_INSTANCE no configurado"),
    ["AzureAd:TenantId"] = companyConfig.AZURE_AD_API_TENANT_ID ?? throw new InvalidOperationException("AZURE_AD_API_TENANT_ID no configurado"),
    ["AzureAd:ClientId"] = companyConfig.AZURE_AD_API_CLIENT_ID ?? throw new InvalidOperationException("AZURE_AD_API_CLIENT_ID no configurado"),
    ["AzureAd:Audience"] = companyConfig.AZURE_AD_API_AUDIENCE ?? companyConfig.AZURE_AD_API_CLIENT_ID
};

var inMemoryConfig = new ConfigurationBuilder()
    .AddInMemoryCollection(azureAdConfig!)
    .Build();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(inMemoryConfig.GetSection("AzureAd"));

// -----------------------------------------------
// 2. AUTORIZACIÓN
// -----------------------------------------------
builder.Services.AddAuthorization();

// -----------------------------------------------
// 3. SERVICIOS PROPIOS
// -----------------------------------------------
builder.Services.AddScoped<PermissionService>();

// -----------------------------------------------
// 4. CONTROLLERS Y JSON
// -----------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = !companyConfig.IS_PRODUCTION;
    });

// -----------------------------------------------
// 5. SWAGGER
// -----------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = $"CMS API - {companyConfig.COMPANY_NAME}",
        Version = "v1",
        Description = $"API REST del System CMS ({environmentName})"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token de Azure AD"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------------------------
// 6. CORS
// -----------------------------------------------
if (!companyConfig.IS_PRODUCTION)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });
}
else
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ProductionCors", policy =>
        {
            policy.WithOrigins("https://CMS.cccn.org", "https://www.cccn.org")
                  .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        });
    });
}

// ================================================================================
// CONFIGURACIÓN DEL PIPELINE
// ================================================================================

var app = builder.Build();

if (!companyConfig.IS_PRODUCTION)
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = $"CMS API - {companyConfig.COMPANY_NAME}";
    });
    Console.WriteLine("📖 Swagger disponible en: /swagger");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

if (!companyConfig.IS_PRODUCTION)
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("ProductionCors");
}

app.UseAuthentication();
app.UseMiddleware<AuditUserMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => new
{
    Status = "Healthy",
    Company = companyConfig.COMPANY_NAME,
    Schema = companyConfig.COMPANY_SCHEMA,
    Environment = environmentName,
    Timestamp = DateTime.UtcNow
}).WithTags("Health");

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  ✅ CMS.API INICIADA - {environmentName,-34} ║");
Console.WriteLine($"║  🌐 URLs: {string.Join(", ", app.Urls).PadRight(46)}║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

app.Run();