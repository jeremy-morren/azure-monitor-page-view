using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using OpenTelemetry;

namespace WpfApp.Processors;

public class AuthTokenProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity data)
    {
        if (MainWindow.AuthToken is not { Token: { } token }) return;
        var userId = GetUserId(token);
        if (userId != null)
            data.SetTag("enduser.id", userId);
    }

    // public void Initialize(ITelemetry telemetry)
    // {
    //     if (MainWindow.AuthToken is not { Token: { } token }) return;
    //     telemetry.Context.User.AuthenticatedUserId = GetUserId(token);
    //     telemetry.Context.Device.Type = "Browser";
    //     telemetry.Context.Device.OperatingSystem = OsInfo.Version;
    //     telemetry.Context.Session.Id = SessionId;
    //     telemetry.Context.Cloud.RoleName = "Client";
    //     telemetry.Context.Cloud.RoleInstance = null;
    // }

    private static string? GetUserId(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        return jsonToken?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    }
}