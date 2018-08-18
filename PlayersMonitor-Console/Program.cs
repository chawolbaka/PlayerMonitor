using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using MinecraftProtocol.Utils;
using MinecraftProtocol.DataType;
using System.Runtime.InteropServices;

namespace PlayersMonitor
{
    
    class Program
    {
        public static Configuration Config;
        public static PlayersManager PlayersManager;
        private delegate PingReply Run();
        private static Statuses Status ;
        private static bool FirstPrint = true;
        private static bool IsWindows = false;
        enum Statuses
        {
            Initializing,
            Monitor,
            ShowAllInfo
        }

        static void Main(string[] args)
        {
            Status = Statuses.Initializing;
            Initializa();

            Ping ping = new Ping(Config.ServerHost, Config.ServerPort);
            Status = Statuses.Monitor;
            Thread PrintThread =  new Thread(StartPrintInfo);
            PrintThread.Start(ping);
            while (true)
            {
                ConsoleKeyInfo Input = Console.ReadKey(true);
                if (Input.Key==ConsoleKey.A&&Status==Statuses.Monitor&&false)
                {
                    Status =  Statuses.ShowAllInfo;
                    //必须等线程结束才能继续,那么问题来啦.我怎么不用死循环来知道线程是不是结束了?
                    Thread.Sleep(Config.SleepTime + 30);
                    if (PrintThread.IsAlive == false)
                    {
                        PrintAllInfo(ping);
                        Console.ReadKey();
                        Status = Statuses.Monitor;
                        PrintThread = new Thread(StartPrintInfo);
                        PrintThread.Start();
                    }
                }
                else if (Input.Key == ConsoleKey.Q || Input.Key == ConsoleKey.Escape)
                {
                    Console.CursorVisible = true;
                    Environment.Exit(0);
                }
            }
        }
        static void StartPrintInfo(object obj)
        {
            Ping Ping = obj as Ping;
            string Tag_S = "", Tag_C = "";
            while (Status == Statuses.Monitor)
            {
                PingReply PingResult = ExceptionHandler(Ping.Send);
                Console.Title = Settings.TitleStyle.
                    Replace("$IP", Config.ServerHost).
                    Replace("$PORT", Config.ServerPort.ToString()).
                    Replace("$PING_TIME", ((float)(PingResult.Time / 10000.0f)).ToString("F2"));
                if (FirstPrint == true)
                {
                    Screen.Clear();
                    Tag_S = Screen.CreateLine("服务端版本:", "");
                    Tag_C = Screen.CreateLine("在线人数:", "");
                    FirstPrint = false;
                }
                Screen.ReviseLineField(GetServerVersionNameColor(PingResult.Version.Name), 1, Tag_S);
                Screen.ReviseLineField($"&f{PingResult.Player.Online}/{PingResult.Player.Max}", 1, Tag_C);
                if (PingResult.Player.Samples!=null)
                {
                
                    foreach (var player in PingResult.Player.Samples)
                    {
                        PlayersManager.Add(player.Name, Guid.Parse(player.Id));
                    }
                }
                else
                {
                    PlayersManager.LifeTimer();
                }
                Thread.Sleep(Config.SleepTime / 2);
            }
            return;
        }
        static void PrintAllInfo(Ping ping)
        {
            Screen.Clear();
            var result = ping.Send();
            Console.WriteLine($"服务端版本:{result.Version.Name}({result.Version.Protocol})");
        }
        static void Initializa()
        {
            Config = Configuration.Load(Environment.GetCommandLineArgs());
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#if (DEBUG == false)
            if (string.IsNullOrWhiteSpace(Config.ServerHost))
            {
                Console.Write("服务器地址:");
                Console.ForegroundColor = ConsoleColor.White;
                string Input = Console.ReadLine();
                Console.ResetColor();
                string reg = @"^\s*(\S+\.\S+)(：|:)([1-9]\d{0,3}|[1-5]\d{0,4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])\s*$";
                while (string.IsNullOrWhiteSpace(Input))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("QAQ好好输入服务器地址,不要输入空格呀...");
                    Console.ResetColor();
                    Console.Write("服务器地址:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Input = Console.ReadLine();
                    Console.ResetColor();
                }
                if (Regex.Match(Input, reg).Success==true)
                {
                    Config.ServerHost = Regex.Replace(Input, reg, "$1");
                    Config.ServerPort = ushort.Parse(Regex.Replace(Input, reg, "$3"));
                }
                else
                    Config.ServerHost = Input;
            }
            if (Config.ServerPort == 0)
            {
                Console.Write("服务器端口:");
                Console.ForegroundColor = ConsoleColor.White;
                string Input = Console.ReadLine();
                Console.ResetColor();
                ushort tmp;
                while (ushort.TryParse(Input,out tmp)==false||tmp==0)
                {
                    Console.Clear();
                    if (string.IsNullOrWhiteSpace(Input))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("请输入一个有效的数字(范围:1-65535)");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"错误:");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("你输入的\"");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"{Input}");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\"不是一个有效的端口号,请重新输入服务器端口号(范围:1-65535)");
                    }
                    Console.ResetColor();
                    Console.Write("服务器端口:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Input = Console.ReadLine();
                    Console.ResetColor();
                }
                Config.ServerPort = tmp;
            }
#endif
            Screen.Clear();
            PlayersManager = new PlayersManager(Config);
        }
        static string GetServerVersionNameColor(string serverVersionName)
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
        static PingReply ExceptionHandler(Run run)
        {
            DateTime? FirstTime=null;
            int RetryTime=1000*8;
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
                catch(SocketException e)
                {
                    Console.Clear();
                    Console.Title = "发生了网路错误...";
                    FirstPrint = true;
                    if (e.ErrorCode==(int)SocketError.HostNotFound)
                    {
                        //我没找到linux上这个错误的错误代码...
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("服务器地址错误(找不到这个地址)");
                        if (IsWindows==true)
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
                            if (IsWindows == true)
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
