using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public enum CommandStatus
    {
        Pending,
        Running,
        Finished,
        Error
    }
}
