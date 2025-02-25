using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[ApiController, Route("api/[controller]")]
public class UsersController(HttpClient client) : Controller
{
    [HttpGet]
    public async Task<User> Index()
    {
        var id = Random.Shared.Next(1, 10);
        var response = await client.GetFromJsonAsync<Response<User>>($"https://reqres.in/api/users/{id}", JsonOptions);
        return response!.Data;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private record Response<T>(T Data);
}

public record User(int Id, string Email, string FirstName, string LastName);