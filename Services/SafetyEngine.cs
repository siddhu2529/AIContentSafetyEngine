using System.Text.Json;
using System.Text.RegularExpressions;
using Sentinel.AISafety.Models;

namespace Sentinel.AISafety.Services;

public interface ISafetyEngine
{
    SafetyResult Evaluate(string content);
}


public class SafetyEngine : ISafetyEngine
{
    private readonly SafetyPolicy _policy;

    public SafetyEngine()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "policy.json");
        var json = File.ReadAllText(path);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _policy = JsonSerializer.Deserialize<SafetyPolicy>(json, options)
                  ?? new SafetyPolicy(new List<SafetyRule>());
    }

    public SafetyResult Evaluate(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return new SafetyResult(true);

        string transformedContent = content;

        // 1. ALWAYS CHECK FOR BLOCKS FIRST
        // This ensures no 'Massage' logic accidentally hides a 'Block' intent
        foreach (var rule in _policy.Rules.Where(r => r.Action == "Block"))
        {
            // Use RegexOptions.Singleline to handle newlines in the prompt
            if (Regex.IsMatch(content, rule.Pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                return new SafetyResult(false, rule.Category, rule.Id, null, "Block", rule.RefusalMessage);
            }
        }

        // 2. APPLY TRANSFORMATIONS (Redact/Massage)
        foreach (var rule in _policy.Rules.Where(r => r.Action != "Block"))
        {
            if (Regex.IsMatch(content, rule.Pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                if (rule.Action == "Redact")
                {
                    transformedContent = Regex.Replace(transformedContent, rule.Pattern, "[REDACTED]", RegexOptions.IgnoreCase);
                }
                else if (rule.Action == "Massage")
                {
                    string header = "### IMPORTANT SAFETY GUIDELINES ### ... USER REQUEST: ";
                    if (!transformedContent.StartsWith("### IMPORTANT"))
                    {
                        transformedContent = header + transformedContent;
                    }
                }
            }
        }

        return new SafetyResult(true, "Allow", null, transformedContent);
    }
}