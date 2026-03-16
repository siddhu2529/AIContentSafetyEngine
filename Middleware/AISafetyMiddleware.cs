using System.Text;
using Sentinel.AISafety.Services;
using System.Text.Json;
using Sentinel.AISafety.Models;
using System.Text.RegularExpressions;

namespace Sentinel.AISafety.Middleware;

public class AISafetyMiddleware(RequestDelegate _next, IHttpClientFactory _httpClientFactory)
{

    public async Task InvokeAsync(HttpContext context, ISafetyEngine safetyEngine, TableStorageService azureLogger)
    {
        if (context.Request.Method == HttpMethods.Post)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            using var jsonDoc = JsonDocument.Parse(body);
            if (jsonDoc.RootElement.TryGetProperty("Prompt", out var promptElement))
            {
                var prompt = promptElement.GetString() ?? string.Empty;
                var promptResult = safetyEngine.Evaluate(prompt);

                // 1. INPUT VALIDATION
                if (!promptResult.IsAllowed)
                {
                    await azureLogger.LogToAzureAsync("Security_Block", promptResult.Category, prompt);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        response = promptResult.RefusalMessage,
                        category = promptResult.Category,
                        is_safe = false
                    });
                    return; 
                }

                var client = _httpClientFactory.CreateClient("OllamaClient");
                var ollamaRequestBody = new
                {
                    model = "dolphin-mistral",
                    prompt = promptResult.ProcessedContent ?? prompt,
                    stream = false
                };

                var response = await client.PostAsJsonAsync("", ollamaRequestBody);
                var ollamaResponseBody = await response.Content.ReadFromJsonAsync<OllamaResponse>();

                if (ollamaResponseBody != null)
                {
                    // 3. OUTPUT VALIDATION
                    var result = safetyEngine.Evaluate(ollamaResponseBody.response);

                    if (!result.IsAllowed)
                    {
                        await azureLogger.LogToAzureAsync("Output_Block", result.Category, ollamaResponseBody.response);
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            response = result.RefusalMessage,
                            category = result.Category,
                            is_safe = false
                        });
                        return;
                    }

                    if (result.ProcessedContent != null)
                    {
                        ollamaResponseBody.response = result.ProcessedContent;
                    }

                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsJsonAsync(ollamaResponseBody);
                    return;
                }
            }
        }

        await _next(context);
    }
}