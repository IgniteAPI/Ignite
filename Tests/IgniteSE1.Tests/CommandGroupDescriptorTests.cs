using InstanceUtils.Services.Commands;
using IgniteAPI.Models.Commands;
using Xunit;

namespace IgniteSE1.Tests
{
    public class CommandGroupDescriptorTests
    {
        private readonly ITestOutputHelper _output;

        public CommandGroupDescriptorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Verifies that the constructor correctly assigns the Name and Description properties.
        /// </summary>
        [Fact]
        public void Constructor_SetsNameAndDescription()
        {
            var group = new CommandGroupDescriptor("server", "Server commands");

            _output.WriteLine($"Name: {group.Name}, Description: {group.Description}");

            Assert.Equal("server", group.Name);
            Assert.Equal("Server commands", group.Description);
        }

        /// <summary>
        /// Verifies that a sub-group added via AddSubGroup can be retrieved by name
        /// and that its Parent reference points back to the root group.
        /// </summary>
        [Fact]
        public void AddSubGroup_IsRetrievableByName()
        {
            var root = new CommandGroupDescriptor("root");
            var child = new CommandGroupDescriptor("child", "A child group");

            root.AddSubGroup(child);

            _output.WriteLine($"SubGroups count: {root.SubGroups.Count}");
            _output.WriteLine($"Child parent: {child.Parent?.Name}");

            Assert.Same(child, root.GetSubGroup("child"));
            Assert.Same(root, child.Parent);
        }

        /// <summary>
        /// Verifies that GetSubGroup returns null when the requested name does not exist.
        /// </summary>
        [Fact]
        public void GetSubGroup_ReturnsNull_WhenNotFound()
        {
            var root = new CommandGroupDescriptor("root");

            var result = root.GetSubGroup("nonexistent");
            _output.WriteLine($"GetSubGroup(\"nonexistent\") returned: {(result == null ? "null" : result.Name)}");

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that sub-group lookup is case-insensitive (e.g. "config" matches "Config").
        /// </summary>
        [Fact]
        public void GetSubGroup_IsCaseInsensitive()
        {
            var root = new CommandGroupDescriptor("root");
            root.AddSubGroup(new CommandGroupDescriptor("Config"));

            _output.WriteLine("Looking up \"config\" and \"CONFIG\" against registered \"Config\"");

            Assert.NotNull(root.GetSubGroup("config"));
            Assert.NotNull(root.GetSubGroup("CONFIG"));
        }

        /// <summary>
        /// Verifies that AddCommand registers the command in the Commands dictionary
        /// and sets its ParentGroup back-reference.
        /// </summary>
        [Fact]
        public void AddCommand_SetsParentGroup()
        {
            var group = new CommandGroupDescriptor("grp");
            var cmd = new CommandDescriptor("start", "Start server", null, typeof(object), CommandTypeEnum.AdminOnly);

            group.AddCommand(cmd);

            _output.WriteLine($"Commands count: {group.Commands.Count}");
            _output.WriteLine($"Command parent group: {cmd.ParentGroup?.Name}");

            Assert.Same(group, cmd.ParentGroup);
            Assert.True(group.Commands.ContainsKey("start"));
        }

        /// <summary>
        /// Verifies that HasCommandTypeOverride is true when a CommandTypeEnum is provided
        /// and false when null is passed.
        /// </summary>
        [Fact]
        public void HasCommandTypeOverride_ReflectsValue()
        {
            var withOverride = new CommandGroupDescriptor("a", "", CommandTypeEnum.AdminOnly);
            var withNull = new CommandGroupDescriptor("b", "", null);

            _output.WriteLine($"WithOverride.HasCommandTypeOverride: {withOverride.HasCommandTypeOverride}");
            _output.WriteLine($"WithNull.HasCommandTypeOverride: {withNull.HasCommandTypeOverride}");

            Assert.True(withOverride.HasCommandTypeOverride);
            Assert.False(withNull.HasCommandTypeOverride);
        }
    }
}
