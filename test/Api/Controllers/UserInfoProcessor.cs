using System.Diagnostics;
using System.Security.Claims;
using OpenTelemetry;

namespace Api.Controllers;

/// <summary>
/// Adds user info to the activity.
/// </summary>
public class UserInfoProcessor : BaseProcessor<Activity>
{
    private readonly IHttpContextAccessor _accessor;

    public UserInfoProcessor(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public override void OnEnd(Activity data)
    {
        try
        {
            if (_accessor.HttpContext?.User is not { Identity.IsAuthenticated: true } user)
                return;

            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = user.FindFirst(user.Identities.First().NameClaimType)?.Value;
            var roles = user.FindAll(user.Identities.First().RoleClaimType).Select(c => c.Value).ToArray();
            data.SetTag("enduser.id", id); //See https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-add-modify?tabs=aspnetcore#set-the-user-id-or-authenticated-user-id
            data.SetTag("user.name", name);
            data.SetTag("user.roles", roles);
        }
        catch (Exception)
        {
            // TODO: Log the exception
            // Nothing we can do
        }
    }
}