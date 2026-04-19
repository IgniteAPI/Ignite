using IgniteSE1.Services;
using InstanceUtils.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IgniteAPI.Attributes;
using IgniteAPI.Models.Commands;

namespace InstanceUtils.Commands.TestCommand
{
    [CommandGroup("utils", "Simple CLI utilities", CommandTypeEnum.AdminOnly)]
    public class TestCommand
    {
        private GameService gameService;
        private ICommandContext ctx;

        public TestCommand(GameService service, ICommandContext ctx) 
        {
            this.gameService = service;
            this.ctx = ctx;
        }


        [Command("ping", "Pings the server")]
        public void Start()
        {
            ctx.Respond("Pong");
        }


        [Command("basedir", "Stops the server")]
        public void Stop()
        {
            ctx.Respond(AppDomain.CurrentDomain.BaseDirectory);
        }


        /*
        [GroupCommandActionAttribute]
        public void Execute(ICommandContext ctx)
        {
            ctx.Respond("Pong");
        }
        */

        
    }
}
