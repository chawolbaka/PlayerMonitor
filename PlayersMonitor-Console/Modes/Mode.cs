using System;
using System.Collections.Generic;
using System.Text;

namespace PlayersMonitor.Modes
{
    public class Mode
    {
        public enum Type
        {
            Chart,
            Monitor
        }
        protected enum Statuses
        {
            Initializing,
            Running,
            Abort
        }
        protected Statuses Status { get; set; }
    }
}
