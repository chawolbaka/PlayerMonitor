using System;
using System.Threading;
using System.Net.Sockets;
using MinecraftProtocol.Utils;
using MinecraftProtocol.DataType;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.Text;
using System.Collections.Generic;
using PlayerMonitor.Configs;
using PlayerMonitor.ConsolePlus;
namespace PlayerMonitor.Modes
{
    public class MonitorPlayer : Mode
    {

        public override string Name { get { return nameof(MonitorPlayer); } }
        public override string Description { get { return "QAQ反正没人用,懒的写介绍了"; } }

        private delegate PingReply Run();
        private MonitorPlayerConfig Config;
        private PlayerManager MainPlayerManager;
        private Ping SLP;
        private bool IsFirstPrint = true;

        public MonitorPlayer(MonitorPlayerConfig config)
        {
            State = States.Initializing;
            Config = config != null ? config : throw new ArgumentNullException(nameof(config));
            MainPlayerManager = new PlayerManager(config);
            //注册玩家上下线的事件
            if (!string.IsNullOrWhiteSpace(Config.RunCommandForPlayerJoin))
            {
                MainPlayerManager.Joined += player =>
                {
                    const string reg = @"^(\S+)( (.*))?$";
                    ProcessStartInfo StartInfo = new ProcessStartInfo();
                    StartInfo.FileName = Regex.Replace(Config.RunCommandForPlayerJoin, reg, "$1");
                    if (Config.RunCommandForPlayerJoin.Contains(" "))
                        StartInfo.Arguments = Regex
                        .Replace(Config.RunCommandForPlayerJoin, reg, "$3")
                        .Replace("$PLAYER_NAME", player.Name)
                        .Replace("$PLAYER_UUID", player.Uuid.ToString());
                    Process.Start(StartInfo);
                };
            }
            if (!string.IsNullOrWhiteSpace(Config.RunCommandForPlayerDisconnected))
            {
                MainPlayerManager.Disconnected += player =>
                {
                    const string reg = @"^(\S+)( (.*))?$";
                    ProcessStartInfo StartInfo = new ProcessStartInfo();
                    StartInfo.FileName = Regex.Replace(Config.RunCommandForPlayerDisconnected, reg, "$1");
                    if (Config.RunCommandForPlayerDisconnected.Contains(" "))
                        StartInfo.Arguments = Regex
                        .Replace(Config.RunCommandForPlayerDisconnected, reg, "$3")
                        .Replace("$PLAYER_NAME", player.Name)
                        .Replace("$PLAYER_UUID", player.Uuid.ToString());
                    Process.Start(StartInfo);
                };
            }
            //解析服务器地址(如果是域名的话)
            try
            {
                SLP = new Ping(Config.ServerHost, Config.ServerPort);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.HostNotFound)
                {
                    Screen.Clear();
                    ColorfullyConsole.WriteLine("&c错误&r:&f你输入的服务器地址不存在");
                    ColorfullyConsole.WriteLine($"&e详细信息&r:&4{se}");
                    Program.Exit(-1);
                }
            }
            State = States.Initialized;
        }
        public override void Start()
        {
            State = States.Running;
            StartPrintInfo(SLP);
        }
        public override void StartAsync()
        {
            State = States.Running;
            Thread PrintThread = new Thread(StartPrintInfo);
            PrintThread.Start(SLP);
        }
        public void Abort()
        {
            State = States.Abort;
        }

        private void StartPrintInfo(object obj)
        {
            Ping SLP = obj as Ping;
            Guid Tag_ONLINE_COUNT = Guid.Empty;
            while (State == States.Running)
            {
                //获取Ping信息
                PingReply PingResult = ExceptionHandler(SLP.Send);
                //如果是null就代表状态已经切换为Abort需要停止循环了
                if (PingResult == null) return;
                //MC在开启期间被Ping会响应一些不完整的包,我觉得处理方法应该给个提示然后sleep几秒后重新Ping(但是我懒的写提示信息了)
                if (PingResult.Version==null||PingResult.Player==null) continue;
                //开始输出信息(这部分代码会导致树莓派下的Screen无法正常输出)
                float? Time = PingResult.Time / 10000.0f;//有点好奇这里我/10000了的话它是null是不是会报错呀...
                Console.Title = Config.WindowTitleStyle.
                    Replace("$IP", Config.ServerHost).
                    Replace("$PORT", Config.ServerPort.ToString()).
                    Replace("$PING_TIME", Time != null ?$"{(float)Time:F2}": $"{-1}");
                if (IsFirstPrint)
                {
                    Screen.Clear();
                    MainPlayerManager.Clear();
                    Screen.CreateLine("服务端版本:", GetServerVersionNameColor(PingResult.Version.Name.Replace('§', '&')));
                    Tag_ONLINE_COUNT = Screen.CreateLine("在线人数:", $"&f{PingResult.Player.Online}/{PingResult.Player.Max}");
                    IsFirstPrint = false;
                }
                Screen.ReviseField($"&f{PingResult.Player.Online}/{PingResult.Player.Max}", 1, Tag_ONLINE_COUNT);
                if (PingResult.Player.Samples != null)
                {
                    foreach (var player in PingResult.Player.Samples)
                    {
                        if (Config.HighlightList.Contains(player.Name))
                            MainPlayerManager.Add(Config.HighlightColor+player.Name.Replace('§', '&'), Guid.Parse(player.Id));
                        else
                            MainPlayerManager.Add(player.Name.Replace('§', '&'), Guid.Parse(player.Id));
                    }
                }
                MainPlayerManager.LifeTimer();
                Thread.Sleep(Config.SleepTime + new Random().Next(0, 256));
            }
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}: 主线程已停止");
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
            while (State != States.Abort)
            {
                PingReply SLPResult = null;
                try
                {
                    SLPResult = run();
                    if (SLPResult != null)
                    {
                        FirstTime = null;
                        TryTick = 0;
                        return SLPResult;
                    }
                    else
                        throw new NullReferenceException("Reply is null");
                }
                catch (SocketException e)
                {
                    //恢复连接后有两种可能性:
                    //1.服务器崩溃
                    //2.客户端网络异常
                    //这边将来我可能会写更好的处理方法,现在只要崩溃了就无脑清空屏幕和玩家列表(玩家列表在FristPrint那边清理)
                    Screen.Clear();
                    IsFirstPrint = true;
                    if (e.SocketErrorCode == SocketError.HostNotFound)
                    {
                        //我没找到linux上这个错误的错误代码...
                        //这边好像不需要处理了?大概是不会到这边才出现错误的吧?
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("服务器地址错误(找不到这个地址)");
                        if (Platform.IsWindows)
                            Console.ReadKey(true);
                        Environment.Exit(-1);

                    }
                    else
                    {
                        PrintTime(ref FirstTime);
                        if (Platform.IsWindows)
                        {
                            Console.Title = $"网络发生了一点错误(qwq不要怕!可能过一会就可以恢复啦)";
                            ColorfullyConsole.WriteLine($"&c错误信息&r:&c{e.Message}&e(&c错误代码&f:&c{e.ErrorCode}&e)");
                        }
                        else
                        {
                            Console.Title = $"发生了网络异常";
                            ColorfullyConsole.WriteLine($"&e详细信息&r:&c{e}");
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
                        string ErrorJson = SLPResult?.ToString();
                        if (!string.IsNullOrWhiteSpace(ErrorJson) &&
                            ErrorJson.Contains("Server is still starting! Please wait before reconnecting"))
                        {
                            if (TryTick > short.MaxValue)
                            {
                                Console.WriteLine("这服务器怎么一直在开启中的,怕是出了什么bug了...");
                                Console.WriteLine($"请把这些信息复制给作者来修bug:{je}");
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
                    ColorfullyConsole.WriteLine("&cjson解析错误&f:&r服务器返回了一个无法被解析的json");
                    if (SLPResult != null)
                    {
                        ColorfullyConsole.WriteLine($"&e无法被解析的json&f:");
                        ColorfullyConsole.WriteLine($"{SLPResult.ToString()}");
                    }
                    ColorfullyConsole.WriteLine($"&e详细信息&r:&c{je}");
                    RetryHandler(ref RetryTime, ref TryTick, MaxTryTick);
                    continue;
                }
                catch (NullReferenceException nre)
                {
                    StandardExceptionHandler(nre, "发生了异常", FirstTime, RetryTime, TryTick, MaxTryTick);
                    continue;
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.WriteLine($"Time:{DateTime.Now}");
                    throw;
                }
            }
            return null;
        }
        //虽然名字这样叫吧,但是其实只是在打印名字而已
        private void StandardExceptionHandler(Exception e, string consoleTitle, DateTime? firstTime, int retryTime, int tryTick, int maxTryTick)
        {
            Console.Title = consoleTitle;
            Screen.Clear();
            IsFirstPrint = true;
            //Print Info
            PrintTime(ref firstTime);
            ColorfullyConsole.WriteLine($"&e详细信息&r:&c{e}");
            RetryHandler(ref retryTime, ref tryTick, maxTryTick);
        }
        private void RetryHandler(ref int retryTime, ref int tick, int maxTick)
        {
            if (tick == 0)
                ColorfullyConsole.WriteLine($"将在&f{(retryTime / 1000.0f):F2}&r秒后尝试重新连接服务器");
            else if (tick < maxTick)
                ColorfullyConsole.WriteLine($"&e已重试&r:&f{tick}次,{(retryTime / 1000.0f):F2}秒后将继续尝试去重新连接服务器");
            else
            {
                Console.WriteLine($"已到达最大重试次数({maxTick})");
                if (Platform.IsWindows)
                    Console.ReadKey(true);
                Environment.Exit(-1);
            }

            //随机重试时间(随便写的)
            if (tick > maxTick / 2)
            {
                retryTime += new Random().Next(233 * 2, 33333 * 3);
                retryTime -= new Random().Next(2, 33333 * 3);
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
                ColorfullyConsole.WriteLine($"&f发生时间&r:&e{firstTime}");
            }
            else
            {
                ColorfullyConsole.WriteLine($"&f发生时间(首次)&r:&e{firstTime}");
                ColorfullyConsole.WriteLine($"&f发生时间(本次)&r:&e{DateTime.Now}");
            }
        }

        //好像其它地方用不到,所以改成内部类了.(可能哪天又会改回去
        private class PlayerManager
        {
            public delegate void PlayerJoinedEventHandler(Player player);
            public delegate void PlayerDisconnectedEventHandler(Player player);
            public event PlayerJoinedEventHandler Joined;
            public event PlayerDisconnectedEventHandler Disconnected;

            private MonitorPlayerConfig Config;
            private List<Player> PlayerList = new List<Player>();

            public PlayerManager(MonitorPlayerConfig config)
            {
                Config = config ?? throw new ArgumentNullException(nameof(config));
            }
            public void Add(string name, Guid uuid)
            {
                int DefPlayerBlood = PlayerList.Count < 12 ? 2 : Config.Blood;
                if (Config == null)
                    throw new Exception("Not Initializtion");
                Player FoundPlayer = PlayerList.Find(p => p.Name==name&&p.Uuid==uuid);
                if (FoundPlayer != null) //如果找到了这个玩家就把它的血恢复到默认值(回血)
                {
                    FoundPlayer.Blood = DefPlayerBlood;
                    //Screen.ReviseLineField($"{GetBloodColor(FoundPlayer.Blood,Config.Blood)}{FoundPlayer.Blood.ToString("D2")}",3,FoundPlayer.ScreenTag);
                }
                else if (FoundPlayer == null)
                {
                    Player NewPlayer = new Player(name, Guid.Parse(uuid.ToString()), DefPlayerBlood);
                    //格式:[玩家索引/玩家剩余生命]Name:玩家名(UUID)
                    NewPlayer.ScreenTag = Screen.CreateLine(
                        "[", $"{PlayerList.Count + 1:D2}", "/", $"{GetBloodColor(NewPlayer.Blood - 1, Config.Blood)}{NewPlayer.Blood - 1:D2}", "]",
                        "Name:", NewPlayer.Name, "(", $"{NewPlayer.Uuid}", ")");
                    PlayerList.Add(NewPlayer);
                    Joined?.Invoke(NewPlayer);
                }
                //LifeTimer();
            }
            public void Clear()
            {
                if (PlayerList.Count > 0)
                    PlayerList.Clear();
            }
            public void LifeTimer()
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    PlayerList[i].Blood--;

                    if (PlayerList[i].Blood == 0)
                    {
                        Player DeadPlayer = PlayerList[i];
                        PlayerList.Remove(PlayerList[i--]);
                        //从屏幕上移除这个玩家&修改其它玩家的序号(屏幕上的)
                        Screen.RemoveLine(DeadPlayer.ScreenTag, true);
                        if (PlayerList.Count > 0)
                        {
                            for (int j = 0; j < PlayerList.Count; j++)
                            {
                                Screen.ReviseField($"{j+1:D2}", 1, PlayerList[j].ScreenTag);
                            }
                        }
                        Disconnected?.Invoke(DeadPlayer);
                    }
                    else
                    {
                        Screen.ReviseField(
                            $"{GetBloodColor(PlayerList[i].Blood, Config.Blood)}{PlayerList[i].Blood:D2}", 3, PlayerList[i].ScreenTag);
                    }
                }
            }
            private void RestoreHealthForAllPlayer(int? blood = null)
            {
                foreach (var Player in PlayerList)
                {
                    Player.Blood = blood ?? Config.Blood;
                    Screen.ReviseField($"&a{Player.Blood:D2)}", 3, Player.ScreenTag);
                }
            }
            private string GetBloodColor(int nowBlood, int maxBlood)
            {
                if (PlayerList.Count <= 12)
                    return "&a";
                if (nowBlood <= 1 || nowBlood <= maxBlood / 100.0f * 30)
                    return "&c";
                else if (nowBlood <= maxBlood / 100.0f * 48)
                    return "&e";
                else
                    return "&a";
            }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var player in PlayerList)
                {
                    sb.AppendLine($"{player}");
                    sb.Append("---------------------------------");
                }
                return sb.ToString();
            }
            public class Player
            {
                public Guid Uuid { get; set; }
                public string Name { get; set; }
                public int Blood { get; set; }
                public Guid ScreenTag { get; set; }

                public Player(string name, Guid uuid, int blood)
                {
                    this.Name = name;
                    this.Uuid = uuid;
                    this.Blood = blood;
                }
                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"PlayerName:{Name}");
                    sb.AppendLine($"uuid:{Uuid}");
                    sb.AppendLine($"Blood:{Blood}");
                    sb.AppendLine($"ScreenTag:{ScreenTag}");
                    return sb.ToString();
                }
            }
        }
        
    }
}
