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
        protected enum States
        {
            Initializing,
            Initialized,
            Running,
            Abort
        }
        protected States State { get; set; }

        public abstract string Name { get;}
        public abstract string Description { get;}
        //public abstract Version Version { get; protected set; }

        public abstract void Start();
        public abstract void StartAsync();
        
    }
}
