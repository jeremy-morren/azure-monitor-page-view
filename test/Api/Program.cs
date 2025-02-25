using AppInsights;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Exporter.Console.Json;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Api;
using Api.Controllers;
using Serilog;
using Serilog.Events;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Identity.Web;
using Npgsql;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddLogging(lb =>
{
    lb.ClearProviders();
    lb.AddOpenTelemetry(b =>
    {
        b.AddJsonConsoleExporter(o => o.Targets = ConsoleExporterOutputTargets.Debug);
    });
});

builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApi(
        o =>
        {
            o.TokenValidationParameters.ValidateAudience = false;
        },
        o =>
        {
            o.Instance = "https://login.microsoftonline.com/";
            o.TenantId = "88e96ab6-6a86-4cc4-9ff1-055e45fd1d3a";
            o.ClientId = "af227a97-db6b-4ffb-bd44-b866e269b6b6";
        });

builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(o =>
    {
        o.ConnectionString = AppInsightsConstants.ConnString;
    })
    .UseOtlpExporter(OtlpExportProtocol.Grpc,
        builder.Environment.IsDevelopment() ? new Uri("http://localhost:4317") : new Uri("http://jaeger:4317"))
    .ConfigureResource(r =>
    {
        r.AddService("Api");

        r.AddDetector(new KubernetesResourceDetector());
    })
    .WithTracing(b => b
        .SetSampler(new AlwaysOnSampler())
        .AddProcessor<UserInfoProcessor>()

        .AddAspNetCoreInstrumentation(o =>
        {
            o.RecordException = true;
            o.Filter = context => context.Request.Method != "OPTIONS";
        })
        .AddHttpClientInstrumentation(o => o.RecordException = true)
        .AddNpgsql()

        .AddJsonConsoleExporter(o => o.Targets = ConsoleExporterOutputTargets.Debug))
    .WithMetrics(b => b.AddNpgsqlInstrumentation());

builder.Host.UseSerilog((_, logger) =>
        logger
            .WriteTo.Console()
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.IdentityModel", LogEventLevel.Error),
    writeToProviders: true);

var app = builder.Build();

app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseRouting();
app.UseAuthentication();

app.MapControllers();

app.Run();