using System;
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
        public override string Name => nameof(Chart);
        public override string Description => "每x分钟记录下玩家的数量,存储成json(仅存储)";

        public bool IsPrint { get; set; } = true;
        public int Interval { get; set; } = 1000 * 30;

        private readonly string Version="1.0.0";
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
        public override void Start()
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
                if (IsPrint)
                    Console.WriteLine($"[{DateTime.Now.ToString()}]玩家数量:{PingResult.Player.Online}");
                WriteData(FileName,PingResult);
                Thread.Sleep(Interval);
            }
        }
        public override void StartAsync() => new Thread(x => Start()).Start();
        
        private void CreateJson(string fileName,PingReply pingInfo)
        {
            var json = new JObject(
                        new JProperty("type", "CHART:PLAYERS_ONLIE"),
                        new JProperty("version",Version),
                        new JProperty("date",DateTime.Now.ToString("yyyy-mm-dd")),
                        new JProperty("server_host",Config.ServerHost),
                        new JProperty("server_port",Config.ServerPort),
                        new JProperty("max_player", pingInfo.Player.Max),
                        new JProperty("data"));
            StreamWriter stream = new StreamWriter(fileName,false,Encoding.UTF8);
            stream.Write(json.ToString(Formatting.None));
            stream.Flush();
            stream.Close();
        }
        private void WriteData(string fileName, PingReply PingInfo)
        {
            try
            {
                string Ping_Pong_Time = PingInfo.Time == null ? "unknown" : PingInfo.Time.ToString();
                var json = JObject.Parse(File.ReadAllText(fileName, Encoding.UTF8));
                JArray DataArray = (JArray)json["data"];
                DataArray.Add(
                    new JObject(
                        new JProperty("time", DateTime.Now.ToString("HH:mm:ss")),
                        new JProperty("online", PingInfo.Player.Online),
                        new JProperty("delay",Ping_Pong_Time)
                        ));
                json["data"] = DataArray;
                StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8);
                stream.Write(json.ToString(Formatting.None));
                stream.Flush();
                stream.Close();
            }
            catch (IOException)
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
