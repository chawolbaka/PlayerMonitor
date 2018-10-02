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
            Initialized,
            Running,
            Abort
        }
        protected Statuses Status { get; set; }

        public abstract string Name { get;}
        public abstract string Description { get;}
        //public abstract Version Version { get; protected set; }

        public abstract void Start();
        public abstract void StartAsync();
        
    }
}
