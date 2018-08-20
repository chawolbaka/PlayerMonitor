﻿using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MinecraftProtocol.Utils;
using MinecraftProtocol.DataType;
using System.Threading;

namespace PlayersMonitor.Modes
{
    public class Chart:Mode
    {
        public bool IsPrint { get; set; } = true;
        public int Interval { get; set; } = 1000 * 30;

        private string DataPath;
        private Configuration Config;
        private Ping ping;

        public Chart(Configuration config,string path)
        {
            Status = Statuses.Initializing;
            Config = config != null ? config : throw new ArgumentNullException(nameof(config));
            DataPath = string.IsNullOrWhiteSpace(path) != true ? path : throw new ArgumentNullException(nameof(path));
            ping = new Ping(Config.ServerHost, Config.ServerPort);
        }
        public void Start()
        {
            Status = Statuses.Running;
            
            while (Status==Statuses.Running)
            {
                PingReply PingResult = ping.Send();
                if (Directory.Exists(DataPath) == false)
                    Directory.CreateDirectory(DataPath);
                string FileName= Path.Combine(DataPath, $"{DateTime.Now.ToString("yyyy-MM-dd")}.json");
                if (File.Exists(FileName) == false)
                    CreateJson(FileName,PingResult);
                if (IsPrint == true)
                    Console.WriteLine($"[{DateTime.Now.ToString()}]玩家数量:{PingResult.Player.Online}");
                WriteData(FileName,PingResult);
                Thread.Sleep(Interval);
            }
        }
        public void StartAsync() => new Thread(x => Start()).Start();
        
        private void CreateJson(string fileName,PingReply pingInfo)
        {
            var json = new JObject(
                        new JProperty("Type", "CHART:PLAYERS_ONLIE"),
                        new JProperty("DateTime",DateTime.Now.ToString("yyyy-mm-dd")),
                        new JProperty("ServerHost",Config.ServerHost),
                        new JProperty("ServerPort",Config.ServerPort),
                        new JProperty("MaxNumberOfPlayer", pingInfo.Player.Max),
                        new JProperty("Data"));
            StreamWriter stream = new StreamWriter(fileName,false,Encoding.UTF8);
            stream.Write(json.ToString(Formatting.None));
            stream.Flush();
            stream.Close();
        }
        private void WriteData(string fileName, PingReply PingInfo)
        {
            try
            {
                var json = JObject.Parse(File.ReadAllText(fileName, Encoding.UTF8));
                JArray DataArray = (JArray)json["Data"];
                DataArray.Add(
                    new JObject(
                        new JProperty("Time", DateTime.Now.ToString("HH:mm:ss")),
                        new JProperty("Online", PingInfo.Player.Online)));
                json["Data"] = DataArray;
                StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8);
                stream.Write(json.ToString(Formatting.None));
                stream.Flush();
                stream.Close();
            }
            catch (IOException e)
            {
                //无视IO错误
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Abort()
        {
            Status = Statuses.Abort;
        }
    }
}
