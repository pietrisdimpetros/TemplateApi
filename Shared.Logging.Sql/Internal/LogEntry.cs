namespace Shared.Logging.Sql.Internal
{
    public record LogEntry(
        DateTimeOffset Timestamp,
        string Level,
        string Category,
        string Message,
        string? Exception,
        string? TraceId,
        string? SpanId,
        string? MachineName
    );
}