using InstanceUtils.Utils;
using Xunit;

namespace IgniteSE1.Tests
{
    public class SerializationUtilsTests
    {
        private readonly ITestOutputHelper _output;

        public SerializationUtilsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class Payload
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        /// <summary>
        /// Verifies that serializing an object to JSON and deserializing it back
        /// produces an equivalent object with identical property values.
        /// </summary>
        [Fact]
        public void Roundtrip_PreservesValues()
        {
            var original = new Payload { Name = "test", Value = 42 };
            byte[] json = SerializationUtils.ToJson(original);
            var result = SerializationUtils.FromJson<Payload>(json);

            _output.WriteLine($"Original: Name={original.Name}, Value={original.Value}");
            _output.WriteLine($"Result:   Name={result.Name}, Value={result.Value}");

            Assert.Equal(original.Name, result.Name);
            Assert.Equal(original.Value, result.Value);
        }

        /// <summary>
        /// Verifies that ToJson produces valid UTF-8 bytes containing the expected
        /// JSON property names and values.
        /// </summary>
        [Fact]
        public void ToJson_ProducesValidUtf8()
        {
            var obj = new Payload { Name = "hello", Value = 1 };
            byte[] bytes = SerializationUtils.ToJson(obj);
            string text = System.Text.Encoding.UTF8.GetString(bytes);

            _output.WriteLine($"Serialized JSON ({bytes.Length} bytes): {text}");

            Assert.Contains("\"Name\"", text);
            Assert.Contains("\"hello\"", text);
        }

        /// <summary>
        /// Verifies that deserializing an empty JSON object produces a Payload
        /// with default values (null for strings, 0 for ints).
        /// </summary>
        [Fact]
        public void FromJson_WithEmptyObject_ReturnsDefaults()
        {
            byte[] json = System.Text.Encoding.UTF8.GetBytes("{}");
            var result = SerializationUtils.FromJson<Payload>(json);

            _output.WriteLine($"Deserialized from empty object: Name={result.Name ?? "(null)"}, Value={result.Value}");

            Assert.Null(result.Name);
            Assert.Equal(0, result.Value);
        }
    }
}
