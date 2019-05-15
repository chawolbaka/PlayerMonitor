using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MinecraftProtocol.Utils;
using MinecraftProtocol.DataType;
using System.Threading;
using System.Linq;
using PlayerMonitor.Configs;

namespace PlayerMonitor.Modes
{
    public class Chart : Mode
    {
        public override string Name => nameof(Chart);
        public override string Description => "每x分钟记录下玩家的数量,存储成json(仅存储)";

        public bool ToPrintInfo { get; set; } = true;
        public int Interval { get; set; } = 1000 * 30;

        //我不知道应该有什么名字,json里面的字段名我可能会修改所以需要一个版本号
        private readonly string Version = "1.0.0";
        private string DataPath;
        private MonitorPlayerConfig Config;
        private Ping ping;

        public Chart(string savePath)
        {
            State = States.Initializing;
            //先这样子用着隔壁的,反正这类也不会被加载到
            Config = new MonitorPlayerConfig(Environment.GetCommandLineArgs().ToList());
            DataPath = string.IsNullOrWhiteSpace(savePath) != true ? savePath : throw new ArgumentNullException(nameof(savePath));
            ping = new Ping(Config.ServerHost,Config.ServerPort);
        }
        public override void Start()
        {
            State = States.Running;

            while (State == States.Running)
            {
                PingReply PingResult = ping.Send();
                if (Directory.Exists(DataPath) == false)
                    Directory.CreateDirectory(DataPath);
                string FileName = Path.Combine(DataPath, $"{DateTime.Now:yyyy-MM-dd)}.json");
                if (File.Exists(FileName) == false)
                    CreateJson(FileName, PingResult);
                if (ToPrintInfo)
                    Console.WriteLine($"[{DateTime.Now}]玩家数量:{PingResult.Player.Online}");
                WriteData(FileName, PingResult);
                Thread.Sleep(Interval);
            }
        }
        public override void StartAsync() => new Thread(x => Start()).Start();

        private void CreateJson(string fileName, PingReply pingInfo)
        {
            var json = new JObject(
                        new JProperty("type", "CHART:PLAYER_ONLIE"),
                        new JProperty("version", Version),
                        new JProperty("date", DateTime.Now.ToString("yyyy-mm-dd")),
                        new JProperty("server_host", Config.ServerHost),
                        new JProperty("server_port", Config.ServerPort),
                        new JProperty("max_player", pingInfo.Player.Max),
                        new JProperty("data"));
            StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8);
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
                        new JProperty("delay", Ping_Pong_Time)
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
            State = States.Abort;
        }
    }
}
