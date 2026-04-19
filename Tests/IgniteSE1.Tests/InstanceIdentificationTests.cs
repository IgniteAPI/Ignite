using InstanceUtils.Utils.Identification;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace IgniteSE1.Tests
{
    public class InstanceIdentificationTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly ITestOutputHelper _output;

        public InstanceIdentificationTests(ITestOutputHelper output)
        {
            _output = output;
            _tempDir = Path.Combine(Path.GetTempPath(), "IgniteTests_" + Guid.NewGuid().ToString("N"));
            _output.WriteLine($"Temp directory: {_tempDir}");
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        /// <summary>
        /// Verifies that a new non-empty GUID is generated and persisted to disk
        /// when no Instance.id file exists.
        /// </summary>
        [Fact]
        public void GetInstanceID_CreatesNewGuid_WhenNoFileExists()
        {
            var ident = new InstanceIdentification(_tempDir);
            var id = ident.InstanceID;

            _output.WriteLine($"Generated ID: {id}");

            Assert.NotEqual(Guid.Empty, id);
            Assert.True(File.Exists(Path.Combine(_tempDir, "Instance.id")));
        }

        /// <summary>
        /// Verifies that two separate InstanceIdentification instances pointing at the
        /// same directory return the same persisted GUID.
        /// </summary>
        [Fact]
        public void GetInstanceID_ReturnsSameGuid_OnSubsequentCalls()
        {
            var ident = new InstanceIdentification(_tempDir);
            var id1 = ident.InstanceID;

            var ident2 = new InstanceIdentification(_tempDir);
            var id2 = ident2.InstanceID;

            _output.WriteLine($"First call: {id1}, Second call: {id2}");

            Assert.Equal(id1, id2);
        }

        /// <summary>
        /// Verifies that an existing Instance.id file is read and its GUID value is returned.
        /// </summary>
        [Fact]
        public void GetInstanceID_ReadsExistingFile()
        {
            var expected = Guid.NewGuid();
            Directory.CreateDirectory(_tempDir);
            File.WriteAllText(Path.Combine(_tempDir, "Instance.id"), expected.ToString());

            var ident = new InstanceIdentification(_tempDir);

            _output.WriteLine($"Expected: {expected}, Actual: {ident.InstanceID}");

            Assert.Equal(expected, ident.InstanceID);
        }

        /// <summary>
        /// Verifies that when the Instance.id file contains invalid data, a new valid
        /// GUID is generated to replace it.
        /// </summary>
        [Fact]
        public void GetInstanceID_RegeneratesGuid_WhenFileContainsInvalidData()
        {
            Directory.CreateDirectory(_tempDir);
            File.WriteAllText(Path.Combine(_tempDir, "Instance.id"), "not-a-guid");

            var ident = new InstanceIdentification(_tempDir);
            var id = ident.InstanceID;

            _output.WriteLine($"Regenerated ID from invalid data: {id}");

            Assert.NotEqual(Guid.Empty, id);
        }
    }
}
