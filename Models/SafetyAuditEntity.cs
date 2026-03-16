using Azure;
using Azure.Data.Tables;
namespace Sentinel.AISafety.Models;

public class SafetyAuditEntity : ITableEntity
{
    // Required by Azure Table Storage
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Your custom audit fields
    public string Direction { get; set; } = default!; 
    public string Category { get; set; } = default!;
    public string ContentSnippet { get; set; } = default!;
    public DateTime CreatedDateTime { get; set; }
}