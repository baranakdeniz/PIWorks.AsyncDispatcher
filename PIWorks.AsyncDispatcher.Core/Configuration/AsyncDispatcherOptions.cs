using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core.Configuration
{
    public sealed class AsyncDispatcherOptions
    {
        public const string SectionName = "AsyncDispatcher";

        [Range(1, 3600)]
        public int DefaultTimeoutSeconds { get; init; } = 60;

        public string InstanceName { get; init; } = "AsyncDispatcher-Worker";

        public bool EnableDetailedLogging { get; init; } = true;
    }
}
