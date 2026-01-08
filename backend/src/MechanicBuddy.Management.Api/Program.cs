using System.Text;
using k8s;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MechanicBuddy.Management.Api.Authorization;
using MechanicBuddy.Management.Api.Configuration;
using MechanicBuddy.Management.Api.Infrastructure;
using MechanicBuddy.Management.Api.Middleware;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MechanicBuddy Management API",
        Version = "v1",
        Description = "Management API for MechanicBuddy SaaS platform - handles tenant provisioning, billing, and administration"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured");
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MechanicBuddy.Management",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MechanicBuddy.Management.Api",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.Requirements.Add(new SuperAdminRequirement()));

    options.AddPolicy("ActiveAdmin", policy =>
        policy.Requirements.Add(new ActiveAdminRequirement()));
});

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, SuperAdminAuthHandler>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, ActiveAdminAuthHandler>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://mechanicbuddy.com" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register Repositories
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IDemoRequestRepository, DemoRequestRepository>();
builder.Services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
builder.Services.AddScoped<IDomainVerificationRepository, DomainVerificationRepository>();
builder.Services.AddScoped<ITenantMetricsRepository, TenantMetricsRepository>();
builder.Services.AddScoped<IBillingEventRepository, BillingEventRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Configure Provisioning Options
builder.Services.Configure<ProvisioningOptions>(
    builder.Configuration.GetSection("Provisioning"));

// Register Services
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<DemoRequestService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<DomainService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<SuperAdminService>();
builder.Services.AddSingleton<JwtService>();

// Register Background Services
builder.Services.AddHostedService<DemoCleanupService>();

// Register Provisioning Services
builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
builder.Services.AddScoped<IHelmService, HelmService>();
builder.Services.AddScoped<IKubernetesClientService, KubernetesClientService>();

// Register Kubernetes Client for KubernetesClientService (only if in cluster or kubeconfig exists)
builder.Services.AddSingleton<IKubernetes>(sp =>
{
    try
    {
        var config = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();
        return new Kubernetes(config);
    }
    catch (Exception ex)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("KubernetesClient");
        logger.LogWarning(ex, "Kubernetes client initialization failed. Running in non-Kubernetes mode.");
        return null!;
    }
});

// Register Cloudflare Client
builder.Services.AddSingleton<ICloudflareClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var cloudflareEnabled = !string.IsNullOrEmpty(config["Cloudflare:ApiToken"]);

    if (cloudflareEnabled)
    {
        return new CloudflareClient(
            config,
            sp.GetRequiredService<ILogger<CloudflareClient>>());
    }
    return new NoOpCloudflareClient(sp.GetRequiredService<ILogger<NoOpCloudflareClient>>());
});

// Register NPM Client
builder.Services.AddSingleton<INpmClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var npmEnabled = !string.IsNullOrEmpty(config["Npm:BaseUrl"]);

    if (npmEnabled)
    {
        return new NpmClient(
            config,
            sp.GetRequiredService<ILogger<NpmClient>>());
    }
    return new NoOpNpmClient(sp.GetRequiredService<ILogger<NoOpNpmClient>>());
});

// Register Database Provisioner
builder.Services.AddSingleton<ITenantDatabaseProvisioner, TenantDatabaseProvisioner>();

// Register Infrastructure Clients
builder.Services.AddSingleton<IKubernetesClient>(sp =>
{
    var kubernetes = sp.GetService<IKubernetes>();
    var cloudflareClient = sp.GetRequiredService<ICloudflareClient>();
    var npmClient = sp.GetRequiredService<INpmClient>();
    var dbProvisioner = sp.GetRequiredService<ITenantDatabaseProvisioner>();
    var config = sp.GetRequiredService<IConfiguration>();

    if (kubernetes == null)
    {
        var logger = sp.GetRequiredService<ILogger<NoOpKubernetesClient>>();
        return new NoOpKubernetesClient(logger, config, dbProvisioner);
    }
    return new KubernetesClient(
        config,
        sp.GetRequiredService<ILogger<KubernetesClient>>(),
        cloudflareClient,
        npmClient,
        dbProvisioner,
        kubernetes);
});
builder.Services.AddSingleton<IStripeClient, StripeClient>();
builder.Services.AddSingleton<IEmailClient, ResendEmailClient>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not configured"),
        name: "postgres",
        tags: new[] { "db", "ready" }
    );

// Add HTTP Client for external services
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MechanicBuddy Management API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });

    app.UseCors("AllowAll");
}
else
{
    app.UseHttpsRedirection();
    app.UseCors("Production");
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLoggingMiddleware>();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Liveness probe should not check dependencies
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();
