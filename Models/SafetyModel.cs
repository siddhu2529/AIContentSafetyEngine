namespace Sentinel.AISafety.Models;

public class SafetyRule
{
    public string Id { get; }
    public string Category { get; }
    public string Pattern { get; }
    public string Action { get; }
    public string RefusalMessage { get; }

    public SafetyRule(string id, string category, string pattern, string action, string refusalMessage)
    {
        Id = id;
        Category = category;
        Pattern = pattern;
        Action = action;
        RefusalMessage = refusalMessage;
    }
}

public class SafetyPolicy
{
    public List<SafetyRule> Rules { get; }

    public SafetyPolicy(List<SafetyRule> rules)
    {
        Rules = rules;
    }
}

public class SafetyResult
{
    public bool IsAllowed { get; }
    public string? Category { get; }
    public string? RuleId { get; }
    public string? ProcessedContent { get; }
    public string? Action { get; }
    public string RefusalMessage { get;}


    public SafetyResult(bool isAllowed, string? category = null, string? ruleId = null, string? processedContent = null, string? action = "Allow", string refusalMessage = "Content blocked by safety policy.")
    {
        IsAllowed = isAllowed;
        Category = category;
        RuleId = ruleId;
        ProcessedContent = processedContent;
        Action = action;
        RefusalMessage = refusalMessage;
    }
}

public class SafetyLog
{
    public DateTime Timestamp { get; }
    public string Direction { get; }
    public string Category { get; }
    public string Snippet { get; }

    public SafetyLog(DateTime timestamp, string direction, string category, string snippet)
    {
        Timestamp = timestamp;
        Direction = direction;
        Category = category;
        Snippet = snippet;
    }
}