
using System;

namespace PlayersMonitor.Modes
{
    public abstract class Mode
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

        public abstract void Start();
        public abstract void StartAsync();

    }
}
