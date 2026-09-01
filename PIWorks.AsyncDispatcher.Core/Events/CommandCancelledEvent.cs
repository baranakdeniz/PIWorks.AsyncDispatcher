using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using MediatR;
namespace PIWorks.AsyncDispatcher.Core.Events
{
    public record CommandCancelledEvent<TKey>(TKey CommandId) : INotification;
   
}
