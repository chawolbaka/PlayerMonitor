using System;
using System.Collections.Generic;
using System.Text;
using MinecraftProtocol.DataType;

namespace PlayersMonitor
{
    public static class PlayerList
    {
        private static Configuration Config;
        private static List<Player> Players = new List<Player>();
        public static void Initializa(Configuration config)
        {
            Config = config;
        }
        public static void Add(string name, string uuid)
        {
            if (Config == null)
                throw new Exception("not initialized");
            int PlayersLengthBuff = Players.Count;
            var result = Players.Find(delegate (Player PF) { return uuid == PF.UUID; });
            if (result == null)
                Players.Add(new Player(name, uuid, -1));
            else
            {

            }
        }

        class Player
        {
            public Player()
            {

            }
            public Player(string name, string uuid,int blood)
            {
                this.Name = name;
                this.UUID = uuid;
                this.Blood = blood;
            }
            public string UUID { get; set; }
            public string Name { get; set; }
            public int Blood { get; set; }
        }
    }

}
