using IgniteAPI.DTOs.Logs;
using InstanceUtils.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
namespace IgniteSE1.Tests
{
    public class LogBufferTests
    {
        private readonly ITestOutputHelper _output;

        public LogBufferTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static LogLine MakeLine(string msg) => new LogLine
        {
            InstanceName = "test",
            LoggerName = "logger",
            Level = "Info",
            Message = msg,
            Timestamp = DateTime.UtcNow
        };

        /// <summary>
        /// Verifies that adding a single entry to the buffer increases the history count.
        /// </summary>
        [Fact]
        public void Add_SingleEntry_AppearsInHistory()
        {
            var buffer = LogBuffer.Instance;
            int before = buffer.GetHistory().Length;

            buffer.Add(MakeLine("hello"));

            int after = buffer.GetHistory().Length;
            _output.WriteLine($"History count before: {before}, after: {after}");

            Assert.True(after > before);
        }

        /// <summary>
        /// Verifies that the OnLog event fires with the correct entry when a log line is added.
        /// </summary>
        [Fact]
        public void OnLog_FiresWhenEntryAdded()
        {
            var buffer = LogBuffer.Instance;
            LogLine received = null;
            buffer.OnLog += l => received = l;

            try
            {
                var entry = MakeLine("event");
                buffer.Add(entry);

                _output.WriteLine($"Received message: {received?.Message ?? "(null)"}");

                Assert.NotNull(received);
                Assert.Equal("event", received.Message);
            }
            finally
            {
                // Clean up event handler (best-effort; static singleton)
            }
        }

        /// <summary>
        /// Verifies that GetHistory returns a new array instance on each call (snapshot semantics).
        /// </summary>
        [Fact]
        public void GetHistory_ReturnsSnapshotArray()
        {
            var buffer = LogBuffer.Instance;
            buffer.Add(MakeLine("snap"));

            var history1 = buffer.GetHistory();
            var history2 = buffer.GetHistory();

            _output.WriteLine($"history1 hash: {history1.GetHashCode()}, history2 hash: {history2.GetHashCode()}");

            // Each call returns a new array instance
            Assert.NotSame(history1, history2);
        }
    }
}
