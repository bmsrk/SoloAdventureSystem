using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;

namespace SoloAdventureSystem.CLI.Logging
{
    public class InMemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<LogEntry> _queue = new();

        public ILogger CreateLogger(string categoryName)
        {
            return new InMemoryLogger(categoryName, _queue);
        }

        public void Dispose()
        {
            // nothing
        }

        /// <summary>
        /// Dequeue all messages currently buffered.
        /// </summary>
        public IEnumerable<LogEntry> DequeueAll()
        {
            var list = new List<LogEntry>();
            while (_queue.TryDequeue(out var entry)) list.Add(entry);
            return list;
        }

        /// <summary>
        /// Enqueue a progress update (used for model download progress reporting).
        /// </summary>
        public void EnqueueProgress(DownloadProgress progress)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = LogLevel.Information,
                Category = "GGUFDownloader",
                Message = $"Download: {progress.PercentComplete}% - {progress.DownloadedMB:F1}/{progress.TotalMB:F1} MB - {progress.SpeedMBPerSecond:F2} MB/s - ETA {progress.FormattedETA}"
            };
            _queue.Enqueue(entry);
        }

        private class InMemoryLogger : ILogger
        {
            private readonly string _category;
            private readonly ConcurrentQueue<LogEntry> _queue;

            public InMemoryLogger(string category, ConcurrentQueue<LogEntry> queue)
            {
                _category = category;
                _queue = queue;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                try
                {
                    var msg = formatter(state, exception);

                    // Filter out noisy native/CUDA logs so the CLI stays clean.
                    // We ignore logs that reference CUDA/NVIDIA internals or common native backend noise.
                    if (ShouldIgnoreLog(msg, _category))
                        return;

                    var entry = new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = logLevel,
                        Category = _category,
                        Message = msg
                    };
                    _queue.Enqueue(entry);
                }
                catch
                {
                    // swallow
                }
            }

            private static bool ShouldIgnoreLog(string? message, string? category)
            {
                return false;
            }
        }

        public record LogEntry
        {
            public DateTime Timestamp { get; init; }
            public LogLevel Level { get; init; }
            public string Category { get; init; } = string.Empty;
            public string Message { get; init; } = string.Empty;
        };
    }
}
