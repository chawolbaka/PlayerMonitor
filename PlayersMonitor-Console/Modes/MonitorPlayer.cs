using System;
using System.Threading;
using System.Net.Sockets;
using MinecraftProtocol.Utils;
using MinecraftProtocol.DataType;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
#if IsDoNet
using System.Drawing;
#endif

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
            try { 
            
                string Tag_S = "", Tag_C = "";
                while (Status == Statuses.Running)
                {
                    PingReply PingResult = ExceptionHandler(Ping.Send);
                    float? Time = PingResult.Time / 10000.0f;
                    Console.Title = Config.TitleStyle.
                        Replace("$IP", Config.ServerHost).
                        Replace("$PORT", Config.ServerPort.ToString()).
                        Replace("$PING_TIME", Time != null ? ((float)Time).ToString("F2") : $"{(~new Random().Next(1, 233)) + 1}");
                    if (IsFirstPrint)
                    {
                        Screen.Clear();
                        Tag_S = Screen.CreateLine("服务端版本:", "");
                        Tag_C = Screen.CreateLine("在线人数:", "");
#if IsDonet
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
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
#endif
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
            catch (Exception e)
            {
                Console.WriteLine($"Time:{DateTime.Now}");
                Console.WriteLine(PlayerManager.ToString());
                throw;
            }
        }
        private string GetServerVersionNameColor(string serverVersionName)
        {
            //绿色代表可能支持显示玩家,黄色代表未知,红色代表不支持
            //(不精确,可能哪天突然这个服务端就不支持了,或者使用了什么插件禁止这种操作了)
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
            int RetryTime = 1000 * 6;
            int TryTick = 0;
            int MaxTryTick = ushort.MaxValue;
            while (true)
            {
                PingReply Result=null;
                try
                {
                    Result = run();
                    FirstTime = null;
                    TryTick = 0;
                    return Result;
                }
                catch (SocketException e)
                {
                    //如果能恢复的话屏幕那边需要重新初始化
                    Screen.Clear();
                    IsFirstPrint = true;

                    //这个我在考虑要不要移动到下面去
                    
                    if (e.ErrorCode == (int)SocketError.HostNotFound)
                    {
                        //我没找到linux上这个错误的错误代码...
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("服务器地址错误(找不到这个地址)");
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            Console.ReadKey(true);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        if (FirstTime == null)
                        {
                            FirstTime = DateTime.Now;
                        }
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            Console.Title = $"网络发生了一点错误(qwq不要怕!可能过一会就可以恢复啦)";
                            Screen.WriteLine($"&f发生时间(首次)&r:&e{FirstTime.ToString()}");
                            Screen.WriteLine($"&f发生时间(本次)&r:&e{DateTime.Now.ToString()}");
                            Screen.WriteLine($"&c错误信息&r:&c{e.Message}&e(&c错误代码&f:&c{e.ErrorCode}&e)");
                        }
                        else
                        {
                            Console.Title = $"发生了网络异常";
                            Screen.WriteLine($"&f发生时间(首次)&r:&e{FirstTime.ToString()}");
                            Screen.WriteLine($"&f发生时间(本次)&r:&e{DateTime.Now.ToString()}");
                            Screen.WriteLine($"&e详细信息&r:&c{e.ToString()}");
                        }
                        Retry(ref RetryTime, ref TryTick, MaxTryTick);
                        continue;
                    }
                }
                catch (JsonException je)
                {
                    IsFirstPrint = true;
                    Console.Title = "";
                    Screen.Clear();
                    if (je is JsonSerializationException)
                    {
                        string ErrorJson  = Result?.ToString();
                        if (!string.IsNullOrWhiteSpace(ErrorJson)&&
                            ErrorJson.Contains("Server is still starting! Please wait before reconnecting"))
                        {
                            if (TryTick>short.MaxValue)
                            {
                                Console.WriteLine("这服务器怎么一直在开启中的,怕是出了什么bug了...");
                                Console.WriteLine($"请把这些信息复制给作者来修bug:{je.ToString()}");
                            }
                            else
                            {

                                Console.WriteLine("服务器正在开启中,程序将暂时16秒等待服务器开启...");
                                Thread.Sleep(1000 * 16);
                            }
                            TryTick++;
                            continue;
                        }
                    }
                    if (FirstTime == null)
                        FirstTime = DateTime.Now;
                    Screen.WriteLine($"&f发生时间(首次)&r:&e{FirstTime.ToString()}");
                    Screen.WriteLine($"&f发生时间(本次)&r:&e{DateTime.Now.ToString()}");
                    Screen.WriteLine("&cjson解析错误&f:&r服务器返回了一个无法被解析的json");
                    if (Result != null)
                    {
                        Screen.WriteLine($"&e无法被解析的json&f:");
                        Screen.WriteLine($"{Result.ToString()}");
                    }
                    Screen.WriteLine($"&e详细信息&r:&c{je.ToString()}");
                    Retry(ref RetryTime, ref TryTick, MaxTryTick);
                    continue;
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
        private void Retry(ref int retryTime, ref int tick,int maxTick)
        {
            if (tick == 0)
                Screen.Write($"将在&f{(retryTime / 1000.0f).ToString("F2")}&r秒后尝试重新连接服务器");
            else if (tick < maxTick)
                Screen.WriteLine($"&e已重试&r:&f{tick}次,{(retryTime / 1000.0f).ToString("F2")}秒后将继续尝试去重新连接服务器");
            else
            {
                Console.WriteLine($"已到达最大重试次数({maxTick})");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Console.ReadKey(true);
                Environment.Exit(-1);
            }

            //随机重试时间
            if (tick > maxTick / 2)
            {
                retryTime += new Random().Next(2333, 33333 * 3);
                retryTime -= new Random().Next(233, 33333 * 3);
            }
            else
            {
                retryTime += new Random().Next(233, 2333 * 3);
                retryTime -= new Random().Next(23, 2333 * 3);
            }
            if (retryTime <= 1000)
                retryTime = 1000 * 6;
            Thread.Sleep(retryTime);
            tick++;
        }
    }
}
