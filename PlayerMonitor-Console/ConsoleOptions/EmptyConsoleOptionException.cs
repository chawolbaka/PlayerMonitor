using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerMonitor.ConsoleOptions
{
    public class EmptyConsoleOptionException:Exception
    {
        public bool HasNext { get; }
        
        public EmptyConsoleOptionException(bool hasNext) => this.HasNext = hasNext;
        public EmptyConsoleOptionException(string message) : base(message) { }
        public EmptyConsoleOptionException(string message, bool hasNext) : base(message) => this.HasNext = hasNext;
    }
}
