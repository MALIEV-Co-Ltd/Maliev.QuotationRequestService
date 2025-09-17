using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HealthChecks.UI.Client;
using Maliev.QuotationRequestService.Api.Configurations;
using Maliev.QuotationRequestService.Api.HealthChecks;
using Maliev.QuotationRequestService.Api.Middleware;
using Maliev.QuotationRequestService.Api.Models;
using Maliev.QuotationRequestService.Api.Services;
using Maliev.QuotationRequestService.Data.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Maliev Quotation Request Service");

    // Load secrets from mounted volume in GKE
    var secretsPath = "/mnt/secrets";
    if (Directory.Exists(secretsPath))
    {
        builder.Configuration.AddKeyPerFile(directoryPath: secretsPath, optional: true);
    }

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Add controllers
    builder.Services.AddControllers();

    // Configure QuotationRequest DbContext
    if (builder.Environment.IsEnvironment("Testing"))
    {
        builder.Services.AddDbContext<QuotationRequestDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
    }
    else
    {
        builder.Services.AddDbContext<QuotationRequestDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("QuotationRequestDbContext"));
        });
    }

    // Configure memory cache - CRITICAL: Simple configuration per CLAUDE.md
    builder.Services.AddMemoryCache();

    // Configure rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Global rate limit for admin operations
        options.AddPolicy("GlobalPolicy", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 100
                }));

        // Quotation request submission rate limit (more restrictive for public)
        options.AddPolicy("QuotationRequestPolicy", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 3
                }));
    });

    // Configure UploadService options
    builder.Services.Configure<UploadServiceOptions>(builder.Configuration.GetSection(UploadServiceOptions.SectionName));

    // Configure development fallbacks for UploadService
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.Configure<UploadServiceOptions>(options =>
        {
            options.BaseUrl = "http://localhost:8080";
            options.TimeoutSeconds = 30;
        });
    }

    // Configure HTTP client for UploadService
    builder.Services.AddHttpClient<IUploadServiceClient, UploadServiceClient>((serviceProvider, client) =>
    {
        var uploadServiceOptions = serviceProvider.GetRequiredService<IOptions<UploadServiceOptions>>().Value;
        client.BaseAddress = new Uri(uploadServiceOptions.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(uploadServiceOptions.TimeoutSeconds);
    });

    // Register services
    builder.Services.AddScoped<IQuotationRequestService, Maliev.QuotationRequestService.Api.Services.QuotationRequestService>();

    // Configure Swagger
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    builder.Services.AddSwaggerGen();

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
            else
            {
                policy.WithOrigins("https://maliev.com", "https://www.maliev.com")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        });
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    });

    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("Database Health Check", tags: new[] { "readiness" });

    var app = builder.Build();

    app.UseHttpsRedirection();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsProduction())
    {
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "quotation-requests/swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                c.SwaggerEndpoint($"/quotation-requests/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
            c.RoutePrefix = "quotation-requests/swagger";
        });
    }

    // MANDATORY: Prometheus metrics
    app.UseRateLimiter();
    app.UseCors();

    app.MapControllers();

    // Health check endpoints
    app.MapGet("/quotation-requests/liveness", () => "Healthy").AllowAnonymous();

    app.MapHealthChecks("/quotation-requests/readiness", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("readiness"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // MANDATORY: Prometheus metrics endpoint

    // Safe database initialization - only for non-production environments
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<QuotationRequestDbContext>();
        try
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
            {
                // Check if connection string is configured
                var connectionString = context.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    if (context.Database.IsRelational())
                    {
                        context.Database.Migrate();
                        Log.Information("Database migration completed for {Environment}", app.Environment.EnvironmentName);
                    }
                    else
                    {
                        context.Database.EnsureCreated();
                        Log.Information("In-memory database created for {Environment}", app.Environment.EnvironmentName);
                    }
                }
                else
                {
                    Log.Warning("No database connection string configured for {Environment} environment", app.Environment.EnvironmentName);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database initialization failed for environment {Environment}", app.Environment.EnvironmentName);
        }
    }

    Log.Information("Maliev Quotation Request Service started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program
{ }
