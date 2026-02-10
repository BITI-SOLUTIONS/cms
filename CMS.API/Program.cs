// ================================================================================
// ARCHIVO: CMS.API/Program.cs
// PROPÓSITO: Configuración y arranque de la API REST del Sistema CMS
// DESCRIPCIÓN: BOOTSTRAP desde connectionstrings.json + Configuración desde BD
//              Lee [ADMIN].[COMPANY] para obtener TODA la configuración
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ACTUALIZADO: 2026-02-10
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
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// ⭐ CONFIGURAR FORWARDED HEADERS PARA TRAEFIK
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 2;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ================================================================================
// FASE 1: DETECTAR AMBIENTE Y CARGAR CONFIGURACIÓN
// ================================================================================
var connectionConfigPath = Path.Combine(builder.Environment.ContentRootPath, "connectionstrings.json");

if (!File.Exists(connectionConfigPath))
{
    throw new FileNotFoundException($"❌ No se encontró: {connectionConfigPath}");
}

Console.WriteLine($"✅ Cargando desde: {connectionConfigPath}");

builder.Configuration.AddJsonFile("connectionstrings.json", optional: false, reloadOnChange: true);

// ⭐ LEER AMBIENTE
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? builder.Configuration["Environment"]
    ?? "Development";

var isDevelopment = environment == "Development";

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  🌍 Ambiente: {environment.PadRight(45)}║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

// ⭐ LEER CONFIGURACIÓN SEGÚN AMBIENTE
var companySchema = builder.Configuration["CompanySchema"]
    ?? throw new InvalidOperationException("❌ 'CompanySchema' no configurado");

var bootstrapConnectionString = builder.Configuration[$"ConnectionStrings:{environment}:DefaultConnection"]
    ?? throw new InvalidOperationException($"❌ ConnectionStrings:{environment}:DefaultConnection no encontrado");

Console.WriteLine($"📂 Schema: {companySchema}");
Console.WriteLine($"🗄️  BD: {(isDevelopment ? "10.0.0.1 (Development)" : "cms-postgres (Production)")}");

// ================================================================================
// FASE 2: CONFIGURAR DbContext (PostgreSQL)
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
    throw new InvalidOperationException($"❌ Error cargando configuración: {ex.Message}", ex);
}

Console.WriteLine($"✅ Compañía: {companyConfig.COMPANY_NAME}");

var environmentName = isDevelopment ? "DEVELOPMENT" : "PRODUCTION";

builder.Services.AddSingleton(companyConfig);

// ================================================================================
// CONFIGURACIÓN DE SERVICIOS
// ================================================================================

// 1. AUTENTICACIÓN (JWT con Azure AD)
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

// 2. AUTORIZACIÓN
builder.Services.AddAuthorization();

// 3. CONTROLLERS Y JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = isDevelopment;
    });

// 4. SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = $"CMS API - {companyConfig.COMPANY_NAME}",
        Version = "v1",
        Description = $"API REST del CMS ({environmentName})"
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

// 5. CORS
if (isDevelopment)
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
            policy.WithOrigins("https://cms.biti-solutions.com")
                  .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        });
    });
}

// ================================================================================
// PIPELINE
// ================================================================================
var app = builder.Build();

app.UseForwardedHeaders();

if (isDevelopment)
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS API v1");
        c.RoutePrefix = "swagger";
    });
    app.UseCors("AllowAll");
    Console.WriteLine("📖 Swagger: /swagger");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseCors("ProductionCors");
}

app.UseHttpsRedirection();
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
Console.WriteLine($"║  ✅ CMS.API INICIADA - {environmentName.PadRight(35)}║");
Console.WriteLine($"║  🌐 URLs: {string.Join(", ", app.Urls).PadRight(46)}║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

app.Run();