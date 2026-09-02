using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Events
{
    public record CommandCancelledEvent<TKey>(TKey commandId) : INotification;

}
