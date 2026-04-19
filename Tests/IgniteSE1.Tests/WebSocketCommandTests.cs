using IgniteAPI.Attributes;
using IgniteAPI.DTOs.WebSockets;
using IgniteAPI.Models.Commands;
using InstanceUtils.Services;
using InstanceUtils.Services.Commands;
using InstanceUtils.Services.Commands.Contexts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace IgniteSE1.Tests
{
    /// <summary>
    /// Tests the full WebSocket command pipeline: command registration, lookup via
    /// dot-path, and execution through <see cref="WebPanelContext"/> — the same path
    /// the WebPanel uses when it sends a command over the socket.
    /// </summary>
    public class WebSocketCommandTests
    {
        private readonly ITestOutputHelper _output;

        public WebSocketCommandTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Test command class

        /// <summary>
        /// A minimal command class used only for testing. It records whether it was
        /// invoked and captures the argument values so the test can assert on them.
        /// </summary>
        [CommandGroup("test", "Test commands", CommandTypeEnum.WebPanel)]
        public class FakeCommand
        {
            public static bool WasInvoked { get; set; }
            public static string ReceivedName { get; set; }

            private readonly ICommandContext _ctx;

            public FakeCommand(ICommandContext ctx)
            {
                _ctx = ctx;
            }

            [Command("echo", "Echoes a greeting")]
            public void Echo([Input("name", "The name to echo")] string name)
            {
                WasInvoked = true;
                ReceivedName = name;
            }

            [Command("noop", "Does nothing")]
            public void Noop()
            {
                WasInvoked = true;
                ReceivedName = null;
            }
        }

        #endregion

        /// <summary>
        /// Builds a <see cref="CommandService"/> with only the <see cref="FakeCommand"/>
        /// type registered and returns a service provider wired for command execution.
        /// </summary>
        private (CommandService cmdService, IServiceProvider provider) BuildTestHarness()
        {
            var services = new ServiceCollection();
            services.AddScoped<CommandContextAccessor>();
            services.AddScoped<ICommandContext>(sp =>
                sp.GetRequiredService<CommandContextAccessor>().context);

            var sp = services.BuildServiceProvider();
            var cmdService = new CommandService(sp);

            // Register only our test command type (avoids scanning AppDomain for game types)
            cmdService.BuildCommandsFromType(typeof(FakeCommand));

            return (cmdService, sp);
        }

        /// <summary>
        /// Creates a <see cref="SocketMsgEnvelope"/> that mirrors what the WebPanel sends
        /// over the WebSocket when a user triggers a command.
        /// </summary>
        private SocketMsgEnvelope MakeEnvelope(string commandPath, object args = null)
        {
            var envelope = new SocketMsgEnvelope(commandPath);

            var json = JsonSerializer.Serialize(args ?? new { });
            envelope.Args = JsonDocument.Parse(json).RootElement;

            return envelope;
        }

        /// <summary>
        /// Verifies that a command registered with <see cref="CommandService.BuildCommandsFromType"/>
        /// can be looked up via its dot-path (group.command) using TryGetCommand.
        /// </summary>
        [Fact]
        public void TryGetCommand_FindsRegisteredCommand()
        {
            var (cmdService, _) = BuildTestHarness();

            bool found = cmdService.TryGetCommand("test.echo", out var cmd);

            _output.WriteLine($"TryGetCommand(\"test.echo\"): found={found}, name={cmd?.Name}");

            Assert.True(found);
            Assert.Equal("echo", cmd.Name);
        }

        /// <summary>
        /// Verifies that TryGetCommand returns false for an unregistered command path.
        /// </summary>
        [Fact]
        public void TryGetCommand_ReturnsFalse_ForUnknownCommand()
        {
            var (cmdService, _) = BuildTestHarness();

            bool found = cmdService.TryGetCommand("test.doesnotexist", out _);

            _output.WriteLine($"TryGetCommand(\"test.doesnotexist\"): found={found}");

            Assert.False(found);
        }

        /// <summary>
        /// Simulates the full WebSocket command flow: build a socket envelope with a
        /// command path and arguments, resolve the command, and execute it through
        /// <see cref="WebPanelContext"/>. Asserts that the command method was invoked
        /// and received the correct argument value.
        /// </summary>
        [Fact]
        public void WebPanelContext_ExecutesCommand_WithArguments()
        {
            FakeCommand.WasInvoked = false;
            FakeCommand.ReceivedName = null;

            var (cmdService, sp) = BuildTestHarness();

            bool found = cmdService.TryGetCommand("test.echo", out var cmd);
            Assert.True(found, "Command 'test.echo' should be registered");

            var envelope = MakeEnvelope("test.echo", new { name = "World" });

            _output.WriteLine($"Envelope command: {envelope.Command}");
            _output.WriteLine($"Envelope args: {envelope.Args}");

            var ctx = new WebPanelContext(cmd, envelope);
            ctx.RunCommand(sp);

            _output.WriteLine($"WasInvoked: {FakeCommand.WasInvoked}");
            _output.WriteLine($"ReceivedName: {FakeCommand.ReceivedName}");

            Assert.True(FakeCommand.WasInvoked, "Command method should have been invoked");
            Assert.Equal("World", FakeCommand.ReceivedName);
        }

        /// <summary>
        /// Verifies that a parameterless command executes successfully through the
        /// WebSocket pipeline without any args object.
        /// </summary>
        [Fact]
        public void WebPanelContext_ExecutesCommand_WithNoArguments()
        {
            FakeCommand.WasInvoked = false;
            FakeCommand.ReceivedName = null;

            var (cmdService, sp) = BuildTestHarness();

            bool found = cmdService.TryGetCommand("test.noop", out var cmd);
            Assert.True(found, "Command 'test.noop' should be registered");

            var envelope = MakeEnvelope("test.noop");

            _output.WriteLine($"Executing parameterless command via WebPanelContext");

            var ctx = new WebPanelContext(cmd, envelope);
            ctx.RunCommand(sp);

            _output.WriteLine($"WasInvoked: {FakeCommand.WasInvoked}");

            Assert.True(FakeCommand.WasInvoked, "Parameterless command should have been invoked");
        }

        /// <summary>
        /// Verifies that the command context type is correctly set to WebPanel
        /// when executing through <see cref="WebPanelContext"/>.
        /// </summary>
        [Fact]
        public void WebPanelContext_HasCorrectCommandType()
        {
            var (cmdService, _) = BuildTestHarness();
            cmdService.TryGetCommand("test.echo", out var cmd);

            var envelope = MakeEnvelope("test.echo", new { name = "test" });
            var ctx = new WebPanelContext(cmd, envelope);

            _output.WriteLine($"CommandTypeContext: {ctx.CommandTypeContext}");
            _output.WriteLine($"CommandName: {ctx.CommandName}");

            Assert.Equal(CommandTypeEnum.WebPanel, ctx.CommandTypeContext);
            Assert.Equal("echo", ctx.CommandName);
        }
    }
}
