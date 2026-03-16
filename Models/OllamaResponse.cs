namespace Sentinel.AISafety.Models;

public class OllamaResponse
{
    // These names must match the JSON keys returned by Ollama's API
    public string model { get; set; } = default!;
    public string response { get; set; } = default!;
    public bool done { get; set; }
}