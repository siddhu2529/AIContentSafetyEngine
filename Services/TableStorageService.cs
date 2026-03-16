using Azure.Data.Tables;
using Sentinel.AISafety.Models;

namespace Sentinel.AISafety.Services;

public class TableStorageService
{
    private readonly TableClient _tableClient;

    public TableStorageService(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("AzureTableStorage");
        // Ensure you have "SentinelLogs" in your appsettings.json
        var tableName = config["SentinelSettings:TableName"] ?? "ContentAuditLogs₹";
        
        _tableClient = new TableClient(connectionString, tableName);
    }

    public async Task LogToAzureAsync(string direction, string category, string content)
    {
        try{
        // Simple logic: Ensure table exists, then add entity
        await _tableClient.CreateIfNotExistsAsync();

        var entity = new SafetyAuditEntity
        {
            PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd"),
            RowKey = Guid.NewGuid().ToString(),
            Direction = direction,
            Category = category,
            ContentSnippet = content.Length > 100 ? content[..100] : content,
            CreatedDateTime = DateTime.UtcNow
        };

        await _tableClient.AddEntityAsync(entity);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error logging to Azure Table Storage: {ex.Message}");
        }
    }
}