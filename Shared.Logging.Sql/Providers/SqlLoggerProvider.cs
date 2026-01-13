using Microsoft.Extensions.Logging;
using Shared.Logging.Sql.Internal;
using System.Diagnostics;

namespace Shared.Logging.Sql.Providers
{
    internal sealed class SqlLoggerProvider(LogBuffer buffer) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new SqlLogger(categoryName, buffer);
        public void Dispose() { }
    }

    internal sealed class SqlLogger(string category, LogBuffer buffer) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var activity = Activity.Current;

            var entry = new LogEntry(
                DateTimeOffset.UtcNow,
                logLevel.ToString(),
                category,
                formatter(state, exception),
                exception?.ToString(),
                activity?.TraceId.ToString(),
                activity?.SpanId.ToString(),
                Environment.MachineName
            );

            buffer.Push(entry);
        }
    }
}