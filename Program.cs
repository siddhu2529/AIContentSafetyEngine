using Sentinel.AISafety.Services;
using Sentinel.AISafety.Middleware;
using Microsoft.AspNetCore.Http;
using System.Text;
using Sentinel.AISafety.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<TableStorageService>();
builder.Services.AddSingleton<ISafetyEngine, SafetyEngine>();
builder.Services.AddHttpClient("OllamaClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/api/generate");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<AISafetyMiddleware>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/ask-ai", (HttpContext context) =>
{
    var ollamaResponse = context.Items["OllamaResponse"] as OllamaResponse;

    if (ollamaResponse is not null && 
        !string.IsNullOrEmpty(ollamaResponse.response))
    {
        return Results.Ok(new { response = ollamaResponse.response });
    }

    return Results.Ok(new { response = string.Empty });
});


app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record ChatRequest(string Prompt);