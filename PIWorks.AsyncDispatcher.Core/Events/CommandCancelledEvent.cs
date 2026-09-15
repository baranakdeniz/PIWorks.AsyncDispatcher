using MediatR;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Events
{
    public record CommandCancelledEvent<TKey>(TKey CommandId,string WorkerId) : ICommandStateEvent<TKey>;

}
