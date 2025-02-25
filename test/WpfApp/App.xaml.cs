using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Windows;
using AppInsights;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Exporter.Console.Json;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WpfApp.Processors;
// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace WpfApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
[SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
public partial class App : Application
{
    public static readonly ActivitySource ActivitySource = new(nameof(WpfApp));

    protected override void OnStartup(StartupEventArgs e)
    {
        Activity.Current = null;
        var activity = ActivitySource.StartActivity("Startup");
        Activity.Current = activity;

        Sdk.CreateTracerProviderBuilder()
            .AddSource(ActivitySource.Name)

            .AddProcessor(new AuthTokenProcessor())
            .AddProcessor(new SessionProcessor())

            .AddHttpClientInstrumentation()
            .AddJsonConsoleExporter(o => o.Targets = ConsoleExporterOutputTargets.Debug)
            .AddAzureMonitorTraceExporter(o =>
            {
                o.ConnectionString = AppInsightsConstants.ConnString;
                o.DisableOfflineStorage = true;
            })
            .AddOtlpExporter()
            .ConfigureResource(r =>
            {
                r.AddService("Client");

                AddDeviceInfo(r);
            })
            .Build();

        using (var view = ActivitySource.StartActivity("PageView"))
        {
            if (view != null)
            {
                view.DisplayName = "Main Window";
            }
            new MainWindow(new HttpClient()).Show();
        }

        // base.OnStartup(e);
    }

    private static void AddDeviceInfo(ResourceBuilder builder) => builder.AddAttributes(
    [
        new("device.type", "Browser"),
        new("os.description", OsInfo.Version)
    ]);
}