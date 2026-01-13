using System.Threading.Channels;

namespace Shared.Logging.Sql.Internal
{
    /// <summary>
    /// A high-performance, non-blocking buffer that decouples
    /// the ILogger (Producer) from the SQL Writer (Consumer).
    /// </summary>
    internal sealed class LogBuffer
    {
        private readonly Channel<LogEntry> _channel;

        public LogBuffer()
        {
            // Unbounded channel ensures the application never blocks waiting for SQL.
            _channel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public void Push(LogEntry entry) => _channel.Writer.TryWrite(entry);

        public IAsyncEnumerable<LogEntry> ReadAllAsync(CancellationToken ct) =>
            _channel.Reader.ReadAllAsync(ct);
    }
}