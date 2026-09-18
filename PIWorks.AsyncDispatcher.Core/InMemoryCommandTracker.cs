using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class InMemoryCommandTracker<TKey> : ICommandTracker<TKey>
    {

        private readonly ConcurrentDictionary<TKey, CommandStateInfo> _states = new();

        public Task<CommandStateInfo?> GetStatusAsync(TKey commandId)
        {//çağırıldığında status gelmeli notnull veya notemptyolmalı!
            //eğer commandyoksa status false gelmeli?
            
           if(_states.TryGetValue(commandId, out var state))
            {
                return Task.FromResult<CommandStateInfo?>(state);
            }
            
            return Task.FromResult<CommandStateInfo?>(null);
        }

        public Task InitializeAsync(TKey commandId)
        {
            var initializeState = new CommandStateInfo
            {
                Status = CommandStatus.Pending,
                WorkerId = null,
                ErrorMessage = null
            };
            _states.TryAdd(commandId, initializeState);

            return Task.CompletedTask;
        }

        public Task UpdateStatusAsync(TKey commandId, CommandStatus status,string? workerId, string errorMessage = null)
        {
         
            
            var newState = new CommandStateInfo
            {
                Status = status,
                WorkerId = workerId,
                ErrorMessage = errorMessage
            };

            _states.AddOrUpdate(commandId, newState,(key,oldState) => newState);
            return Task.CompletedTask;
        }

        public Task<int> GetRunningCommandsCountAsync()
        {
            return Task.FromResult(_states.Count(x => x.Value.Status == CommandStatus.Running));
        }


    }

    }

