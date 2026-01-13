namespace Shared.Logging.Sql.Internal
{
    internal record LogEntry(
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