using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core.Events
{
    public record CommandErrorEvent<TKey>( TKey CommandId, string AppName, string WorkerId, string ErrorMessage) : ICommandStateEvent<TKey>;
}
