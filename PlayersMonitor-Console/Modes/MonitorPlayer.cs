using System;
using System.Threading;
using System.Net.Sockets;
using MinecraftProtocol.Utils;
using MinecraftProtocol.DataType;
using Newtonsoft.Json;
#if !DoNet
using System.Runtime.InteropServices;
#endif
#if DoNet
using System.Drawing;
using System.IO;
#endif

namespace PlayersMonitor.Modes
{
    public class MonitorPlayer:Mode
    {
#if !DoNet
        private static bool IsWindows { get { return RuntimeInformation.IsOSPlatform(OSPlatform.Windows); } }
#elif Windows
        private static bool IsWindows { get { return true; } }
#else
        private static bool IsWindows { get { return false; } }
#endif

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
            try
            {
                ping = new Ping(Config.ServerHost, Config.ServerPort);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode==SocketError.HostNotFound)
                {
                    Screen.Clear();
                    Screen.WriteLine("&c错误&r:&f你输入的服务器地址不存在");
                    Screen.WriteLine($"&e详细信息&r:&4{se.ToString()}");
                    if (IsWindows)
                        Console.ReadKey(true);
                    Environment.Exit(-1);
                }
            }
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
                //获取Ping信息
                PingReply PingResult = ExceptionHandler(Ping.Send);
                if (PingResult == null) return;
                //开始输出信息
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
#if DoNet
                    if (!string.IsNullOrWhiteSpace(PingResult.Icon))
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
            while (Status!= Statuses.Abort)
            {
                PingReply Result=null;
                try
                {
                    Result = run();
                    if (Result!=null)
                    {
                        FirstTime = null;
                        TryTick = 0;
                        return Result;
                    }
                    else 
                        throw new NullReferenceException("Reply is null");
                }
                catch (SocketException e)
                {
                    //如果能恢复的话屏幕那边需要重新初始化,所以这边清理(初始化)一下
                    Screen.Clear();
                    IsFirstPrint = true;
                    if (e.SocketErrorCode == SocketError.HostNotFound)
                    {
                        //我没找到linux上这个错误的错误代码...
                        //这边好像不需要处理了?大概是不会到这边才出现错误的吧?
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("服务器地址错误(找不到这个地址)");
                        if (IsWindows)
                            Console.ReadKey(true);
                        Environment.Exit(-1);

                    }
                    else
                    {
                        PrintTime(ref FirstTime);
                        if (IsWindows)
                        {
                            Console.Title = $"网络发生了一点错误(qwq不要怕!可能过一会就可以恢复啦)";
                            Screen.WriteLine($"&c错误信息&r:&c{e.Message}&e(&c错误代码&f:&c{e.ErrorCode}&e)");
                        }
                        else
                        {
                            Console.Title = $"发生了网络异常";
                            Screen.WriteLine($"&e详细信息&r:&c{e.ToString()}");
                        }
                        RetryHandler(ref RetryTime, ref TryTick, MaxTryTick);
                        continue;
                    }
                }
                catch (JsonException je)
                {
                    IsFirstPrint = true;
                    Console.Title = string.Empty;
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
                    PrintTime(ref FirstTime);
                    Screen.WriteLine("&cjson解析错误&f:&r服务器返回了一个无法被解析的json");
                    if (Result != null)
                    {
                        Screen.WriteLine($"&e无法被解析的json&f:");
                        Screen.WriteLine($"{Result.ToString()}");
                    }
                    Screen.WriteLine($"&e详细信息&r:&c{je.ToString()}");
                    RetryHandler(ref RetryTime, ref TryTick, MaxTryTick);
                    continue;
                }
                catch(NullReferenceException nre)
                {
                    StandardExceptionHandler(nre, "发生了异常", FirstTime, RetryTime, TryTick, MaxTryTick);
                    continue;
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.WriteLine($"Time:{DateTime.Now.ToString()}");
                    throw;
                }
            }
            return null;
        }
        //虽然名字这样叫吧,但是其实只是在打印名字而已
        private void StandardExceptionHandler(Exception e,string consoleTitle, DateTime? firstTime,int retryTime,int tryTick,int maxTryTick)
        {
            Console.Title = consoleTitle;
            Screen.Clear();
            IsFirstPrint = true;
            //Print Info
            PrintTime(ref firstTime);
            Screen.WriteLine($"&e详细信息&r:&c{e.ToString()}");
            RetryHandler(ref retryTime, ref tryTick, maxTryTick);
        }
        private void RetryHandler(ref int retryTime, ref int tick,int maxTick)
        {
            if (tick == 0)
                Screen.Write($"将在&f{(retryTime / 1000.0f).ToString("F2")}&r秒后尝试重新连接服务器");
            else if (tick < maxTick)
                Screen.WriteLine($"&e已重试&r:&f{tick}次,{(retryTime / 1000.0f).ToString("F2")}秒后将继续尝试去重新连接服务器");
            else
            {
                Console.WriteLine($"已到达最大重试次数({maxTick})");
                if (IsWindows)
                    Console.ReadKey(true);
                Environment.Exit(-1);
            }

            //随机重试时间(随便写的)
            if (tick > maxTick / 2)
            {
                retryTime += new Random().Next(233 * 2, 33333 * 3);
                retryTime -= new Random().Next(2, 33333*3);
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
            Console.WriteLine("时间到,正在重试...");
        }
        private void PrintTime(ref DateTime? firstTime)
        {
            if (firstTime == null)
            {
                firstTime = DateTime.Now;
                Screen.WriteLine($"&f发生时间&r:&e{firstTime.ToString()}");
            }
            else
            {
                Screen.WriteLine($"&f发生时间(首次)&r:&e{firstTime.ToString()}");
                Screen.WriteLine($"&f发生时间(本次)&r:&e{DateTime.Now.ToString()}");
            }
        }
    }
}
