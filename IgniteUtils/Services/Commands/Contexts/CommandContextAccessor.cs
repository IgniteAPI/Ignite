using System;
using System.Collections.Generic;
using System.Text;
using IgniteAPI.Models.Commands;

namespace InstanceUtils.Services.Commands.Contexts
{
    public class CommandContextAccessor
    {
        public ICommandContext context { get; set; }
    }
}
