using InstanceUtils.Services.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using IgniteAPI.Models.Commands;

namespace IgniteAPI.Models.Commands
{
    public interface ICommandContext
    {
        CommandTypeEnum CommandTypeContext { get; }   

        string CommandName { get; }

        CommandDescriptor Command { get; }




        void Respond(string response);

        void RespondLine(string response);

    }
}
