using System.ComponentModel;
using System.Data;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Api.Controllers;

[ApiController, Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    [HttpGet("")]
    public async IAsyncEnumerable<City> Index([FromQuery, DefaultValue(3)] int limit)
    {
        const string connString = "Host=localhost;Port=5432;Database=pagila;Username=postgres;Password=postgres";

        await using var connection = new NpgsqlConnection(connString);
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * from public.\"city\" ORDER BY RANDOM() LIMIT @limit";
        command.Parameters.AddWithValue("limit", limit);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            yield return new City(
                reader.GetInt32("city_id"),
                reader.GetString("city"));
    }

    [PublicAPI]
    public record City(int Id, string Name);
}