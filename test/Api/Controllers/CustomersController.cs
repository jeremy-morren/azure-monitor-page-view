using System.ComponentModel;
using System.Data;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Api.Controllers;

[ApiController, Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpGet("")]
    public async IAsyncEnumerable<Customer> Index([FromQuery, DefaultValue(3)] int limit)
    {
        const string connString = "Server=(localdb)\\mssqllocaldb;Database=AdventureWorks2017;Integrated Security=true";

        await using var connection = new SqlConnection(connString);
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP(@limit) CustomerId, CompanyName, Phone FROM SalesLT.Customer ORDER BY newid()";
        command.Parameters.AddWithValue("limit", limit);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            yield return new Customer(
                reader.GetInt32("CustomerId"),
                GetString(reader, "CompanyName"),
                GetString(reader, "Phone"));
    }

    private static string? GetString(SqlDataReader reader, string name) => reader.IsDBNull(name) ? null : reader.GetString(name);

    [PublicAPI]
    public record Customer(int Id, string? CompanyName, string? Phone);
}