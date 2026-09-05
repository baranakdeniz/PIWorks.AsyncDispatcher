using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICommandTracker<TKey>
    {

        Task InitializeAsync(TKey commandId);
        Task UpdateStatusAsync(TKey commandId, CommandStatus status, string errorMessage = null);
        Task<CommandStateInfo?> GetStatusAsync(TKey commandId);
        Task<int> GetRunningCommandsCountAsync();
    }
}
