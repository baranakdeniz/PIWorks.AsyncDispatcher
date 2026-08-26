using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CommandEnvelope<TKey>
    {
     
        public TKey CommandId { get; init; }

       
        public object Command { get; init; }

    
        public Type CommandType { get; init; }
    }
}
