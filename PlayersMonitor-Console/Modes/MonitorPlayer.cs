using System;
using System.Threading;
using System.Net.Sockets;
using MinecraftProtocol.Utils;
using MinecraftProtocol.DataType;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

namespace PlayersMonitor.Modes
{
    public class MonitorPlayer:Mode
    {
        private delegate PingReply Run();
        private Configuration Config;
        private PlayersManager PlayerManager;
        private static bool IsFirstPrint = true;
        private Ping ping;

        public MonitorPlayer(Configuration config, PlayersManager manager)
        {
            Status = Statuses.Initializing;
            Config = config != null ? config : throw new ArgumentNullException(nameof(config));
            PlayerManager = manager != null ? manager : throw new ArgumentNullException(nameof(manager));
            ping = new Ping(Config.ServerHost, Config.ServerPort);
        }
        public void Start()
        {
            Status = Statuses.Running;
            StartPrintInfo(ping);
        }
        public void StartAsync()
        {
            Status = Statuses.Running;
            Thread PrintThread = new Thread(StartPrintInfo);
            PrintThread.Start(ping);
        }
        public void Abort()
        {
            Status = Statuses.Abort;
        }

        private void StartPrintInfo(object obj)
        {
            Ping Ping = obj as Ping;
            string Tag_S = "", Tag_C = "";
            while (Status == Statuses.Running)
            {
                PingReply PingResult = ExceptionHandler(Ping.Send);
                float? Time = PingResult.Time / 10000.0f;
                Console.Title = Config.TitleStyle.
                    Replace("$IP", Config.ServerHost).
                    Replace("$PORT", Config.ServerPort.ToString()).
                    Replace("$PING_TIME",Time!=null?((float)Time).ToString("F2"):$"{(~new Random().Next(1, 233))+1}");
                if (IsFirstPrint == true)
                {
                    Screen.Clear();
                    Tag_S = Screen.CreateLine("服务端版本:", "");
                    Tag_C = Screen.CreateLine("在线人数:", "");
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true &&
                        !string.IsNullOrWhiteSpace(PingResult.Icon))
                    {
                        byte[] Icon_bytes = Convert.FromBase64String(
                            PingResult.Icon.Replace("data:image/png;base64,", ""));
                        using (MemoryStream ms = new MemoryStream(Icon_bytes))
                        {
                            try {
                                Bitmap Icon = new Bitmap(ms);
                                //不知道为什么好像用不了,可能.net core不支持这个东西?
                                //(到时候编译.net 版本的看看有不有效果吧,没有的话就删除这个功能)
                                WinAPI.SetConsoleIcon(Icon.GetHicon());
                            } catch { throw; }
                        }
                    }
                    IsFirstPrint = false;
                }
                Screen.ReviseField(GetServerVersionNameColor(PingResult.Version.Name.Replace('§', '&')), 1, Tag_S);
                Screen.ReviseField($"&f{PingResult.Player.Online}/{PingResult.Player.Max}", 1, Tag_C);
                if (PingResult.Player.Samples != null)
                {
                    foreach (var player in PingResult.Player.Samples)
                    {
                        PlayerManager.Add(player.Name.Replace('§', '&'), Guid.Parse(player.Id));
                    }
                }
                PlayerManager.LifeTimer();
                Thread.Sleep(Config.SleepTime + new Random().Next(0, 256));
            }
            return;
        }
        private string GetServerVersionNameColor(string serverVersionName)
        {
            //绿色代表支持显示玩家,黄色代表未知,红色代表不支持(不精确,可能哪天突然这个服务端就不支持了)

            if (serverVersionName.ToLower().Contains("spigot"))
                return $"&a{serverVersionName}";
            else if (serverVersionName.ToLower().Contains("thermos"))
                return $"&c{serverVersionName}";
            else if (serverVersionName.ToLower().Contains("bungeecord"))
                return $"&c{serverVersionName}";
            else
                return $"&e{serverVersionName}";
        }
        private PingReply ExceptionHandler(Run run)
        {
            DateTime? FirstTime = null;
            int RetryTime = 1000 * 8;
            int TryTick = 0;
            while (true)
            {
                try
                {
                    PingReply tmp = run();
                    FirstTime = null;
                    TryTick = 0;
                    return tmp;
                }
                catch (SocketException e)
                {
                    Console.Clear();
                    Console.Title = "发生了网路错误...";
                    IsFirstPrint = true;
                    if (e.ErrorCode == (int)SocketError.HostNotFound)
                    {
                        //我没找到linux上这个错误的错误代码...
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("服务器地址错误(找不到这个地址)");
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true)
                            Console.ReadKey(true);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        if (FirstTime == null)
                        {
                            FirstTime = DateTime.Now;
                            Console.WriteLine($"Time:{FirstTime.ToString()}");
                            Console.WriteLine($"ErrorMessage:{e.Message}(ErrorCode:{e.ErrorCode})");
                        }
                        else
                        {
                            Console.WriteLine($"首次发生错误的时间:{FirstTime.ToString()}");
                            Console.WriteLine($"本次发生错误的时间:{DateTime.Now.ToString()}");
                            Console.WriteLine($"错误信息:{e.Message}(错误代码:{e.ErrorCode})");
                        }
                        if (TryTick == 0)
                            Console.WriteLine($"将在{(RetryTime / 1000.0f).ToString("F2")}秒后重试");
                        else if (TryTick > ushort.MaxValue)
                        {
                            Console.WriteLine($"已到达最大重试次数({ushort.MaxValue})");
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true)
                                Console.ReadKey(true);
                            Environment.Exit(-1);
                        }
                        else
                            Console.WriteLine($"将在{(RetryTime / 1000.0f).ToString("F2")}秒后重试(已重试{TryTick}次)");
                        TryTick++;
                        Thread.Sleep(RetryTime);
                        continue;
                    }
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.Title = "Error";
                    Console.WriteLine($"Time:{DateTime.Now.ToString()}");
                    throw;
                }
            }
        }

    }
}
