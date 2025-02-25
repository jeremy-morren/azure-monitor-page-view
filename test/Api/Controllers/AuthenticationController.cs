using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController, Route("api/[controller]/[action]")]
public class AuthenticationController : Controller
{
    [HttpGet]
    public IActionResult Me()
    {
        if (User is not { Identity.IsAuthenticated: true })
            return Unauthorized();
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = User.FindFirst(User.Identities.First().NameClaimType)?.Value;
        return Ok(new UserInfo(id, name));
    }
}

public record UserInfo(string? Id, string? Name);