using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core.Events
{
    public record CommandPendingEvent<TKey>(
    TKey CommandId) : ICommandStateEvent<TKey>;
}
