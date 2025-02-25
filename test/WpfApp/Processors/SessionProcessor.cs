using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.IdentityModel.Abstractions;
using OpenTelemetry;

namespace WpfApp.Processors;

/// <summary>
/// Sets the session id for the activity.
/// </summary>
public class SessionProcessor : BaseProcessor<Activity>
{
    private static readonly string SessionId = $"{DateTime.UtcNow:yyyyMMddHHmmss}/{Environment.MachineName}";

    public override void OnEnd(Activity data)
    {
        data.SetTag("ai.session.id", SessionId);
    }
}