using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using MinecraftProtocol.Utils;

namespace PlayersMonitor.Modes
{
    class Chart:Mode
    {
        private string Path;

        public Chart(string path)
        {
            Status = Statuses.Initializing;
            Path = string.IsNullOrWhiteSpace(path) != true ? path : 
                throw new ArgumentNullException(nameof(path));
        }
        public void Start()
        {
            
            Status = Statuses.Running;
        }


        public void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
