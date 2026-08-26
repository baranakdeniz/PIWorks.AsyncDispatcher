using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public readonly struct CommandStateInfo
    {
        public CommandStatus Status { get; init; }
        public string ErrorMessage { get; init; }
    }
}
