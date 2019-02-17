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
using System.Linq;
using PlayersMonitor.Configs;
#if DoNet
using System.Drawing;
using System.IO;
#endif

namespace PlayersMonitor.Modes
{
    public class MonitorPlayer : Mode
    {

        public override string Name { get { return nameof(MonitorPlayer); } }
        public override string Description { get { return "QAQ反正没人用,懒的写介绍了"; } }

        private delegate PingReply Run();
        private MonitorPlayerConfig Config;
        private PlayersManager MainPlayerManager;
        private Ping Ping;
        private bool IsFirstPrint = true;

        public MonitorPlayer(MonitorPlayerConfig config)
        {
            State = States.Initializing;
            Config = config != null ? config : throw new ArgumentNullException(nameof(config));
            MainPlayerManager = new PlayersManager(config);
            //注册玩家上下线的事件
            if (!string.IsNullOrWhiteSpace(Config.RunCommandForPlayerJoin))
            {
                MainPlayerManager.JoinedEvnt += player =>
                {
                    string reg = @"^(\S+)( (.*))?$";
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
                MainPlayerManager.DisconnectedEvent += player =>
                {
                    string reg = @"^(\S+)( (.*))?$";
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
                Ping = new Ping(Config.ServerHost, Config.ServerPort);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.HostNotFound)
                {
                    Screen.Clear();
                    Screen.WriteLine("&c错误&r:&f你输入的服务器地址不存在");
                    Screen.WriteLine($"&e详细信息&r:&4{se.ToString()}");
                    if (SystemInfo.IsWindows)
                        Console.ReadKey(true);
                    Environment.Exit(-1);
                }
            }
            State = States.Initialized;
        }
        public override void Start()
        {
            State = States.Running;
            StartPrintInfo(Ping);
        }
        public override void StartAsync()
        {
            State = States.Running;
            Thread PrintThread = new Thread(StartPrintInfo);
            PrintThread.Start(Ping);
        }
        public void Abort()
        {
            State = States.Abort;
        }

        private void StartPrintInfo(object obj)
        {
            Ping Ping = obj as Ping;
            string Tag_SERVER_VERSION = "", Tag_ONLINE_COUNT = "";
            while (State == States.Running)
            {
                //获取Ping信息
                PingReply PingResult = ExceptionHandler(Ping.Send);
                if (PingResult == null) return;
                //开始输出信息
                float? Time = PingResult.Time / 10000.0f;//有点好奇这里我/10000了的话它是null是不是会报错呀...
                Console.Title = Config.WindowTitleStyle.
                    Replace("$IP", Config.ServerHost).
                    Replace("$PORT", Config.ServerPort.ToString()).
                    Replace("$PING_TIME", Time != null ? ((float)Time).ToString("F2") : $"{short.MinValue}");
                if (IsFirstPrint)
                {
                    Screen.Clear();
                    MainPlayerManager.Clear();
                    Tag_SERVER_VERSION = Screen.CreateLine("服务端版本:", "");
                    Tag_ONLINE_COUNT = Screen.CreateLine("在线人数:", "");
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
                Screen.ReviseField(GetServerVersionNameColor(PingResult.Version.Name.Replace('§', '&')), 1, Tag_SERVER_VERSION);
                Screen.ReviseField($"&f{PingResult.Player.Online}/{PingResult.Player.Max}", 1, Tag_ONLINE_COUNT);
                if (PingResult.Player.Samples != null)
                {
                    foreach (var player in PingResult.Player.Samples)
                    {
                        MainPlayerManager.Add(player.Name.Replace('§', '&'), Guid.Parse(player.Id));
                    }
                }
                MainPlayerManager.LifeTimer();
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
            while (State != States.Abort)
            {
                PingReply Result = null;
                try
                {
                    Result = run();
                    if (Result != null)
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
                        if (SystemInfo.IsWindows)
                            Console.ReadKey(true);
                        Environment.Exit(-1);

                    }
                    else
                    {
                        PrintTime(ref FirstTime);
                        if (SystemInfo.IsWindows)
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
                        string ErrorJson = Result?.ToString();
                        if (!string.IsNullOrWhiteSpace(ErrorJson) &&
                            ErrorJson.Contains("Server is still starting! Please wait before reconnecting"))
                        {
                            if (TryTick > short.MaxValue)
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
                catch (NullReferenceException nre)
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
        private void StandardExceptionHandler(Exception e, string consoleTitle, DateTime? firstTime, int retryTime, int tryTick, int maxTryTick)
        {
            Console.Title = consoleTitle;
            Screen.Clear();
            IsFirstPrint = true;
            //Print Info
            PrintTime(ref firstTime);
            Screen.WriteLine($"&e详细信息&r:&c{e.ToString()}");
            RetryHandler(ref retryTime, ref tryTick, maxTryTick);
        }
        private void RetryHandler(ref int retryTime, ref int tick, int maxTick)
        {
            if (tick == 0)
                Screen.WriteLine($"将在&f{(retryTime / 1000.0f).ToString("F2")}&r秒后尝试重新连接服务器");
            else if (tick < maxTick)
                Screen.WriteLine($"&e已重试&r:&f{tick}次,{(retryTime / 1000.0f).ToString("F2")}秒后将继续尝试去重新连接服务器");
            else
            {
                Console.WriteLine($"已到达最大重试次数({maxTick})");
                if (SystemInfo.IsWindows)
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
                Screen.WriteLine($"&f发生时间&r:&e{firstTime.ToString()}");
            }
            else
            {
                Screen.WriteLine($"&f发生时间(首次)&r:&e{firstTime.ToString()}");
                Screen.WriteLine($"&f发生时间(本次)&r:&e{DateTime.Now.ToString()}");
            }
        }

        public void ReplyHandler(PingReply pingReply)
        {
            throw new NotImplementedException();
        }
        

        //好像其它地方用不到,所以改成内部类了.(可能哪天又会改回去
        private class PlayersManager
        {
            public delegate void PlayerJoinedEvntHandler(Player player);
            public delegate void PlayerDisconnectedEvntHandler(Player player);
            public event PlayerJoinedEvntHandler JoinedEvnt;
            public event PlayerDisconnectedEvntHandler DisconnectedEvent;

            public bool? IsOnlineMode;

            private MonitorPlayerConfig Config;
            private List<Player> PlayersList = new List<Player>();

            public PlayersManager(MonitorPlayerConfig config)
            {
                Config = config ?? throw new ArgumentNullException(nameof(config));
            }
            public void Add(string name, Guid uuid)
            {
                int DefPlayerBlood = PlayersList.Count < 12 ? 2 : Config.Blood;
                if (Config == null)
                    throw new Exception("Not Initializtion");
                Player FoundPlayer = PlayersList.Find(x => x.Uuid.ToString().Replace("-", "") == uuid.ToString().Replace("-", ""));
                if (FoundPlayer != null) //如果找到了这个玩家就把它的血恢复到默认值(回血)
                {
                    FoundPlayer.Blood = DefPlayerBlood;
                    //Screen.ReviseLineField($"{GetBloodColor(FoundPlayer.Blood,Config.Blood)}{FoundPlayer.Blood.ToString("D2")}",3,FoundPlayer.ScreenTag);
                }
                else if (FoundPlayer == null)
                {
                    Player NewPlayer = new Player(name, Guid.Parse(uuid.ToString()), DefPlayerBlood);
                    if (PlayersList.Count == 0)
                    {
                        Thread t = new Thread(iom => IsOnlineMode = NewPlayer.IsOnlineMode());
                        t.Start();
                    }

                    //格式:[玩家索引/玩家剩余生命]Name:玩家名(UUID)
                    NewPlayer.ScreenTag = Screen.CreateLine(
                        "[", (PlayersList.Count + 1).ToString("D2"), "/", $"{GetBloodColor(NewPlayer.Blood - 1, Config.Blood)}{(NewPlayer.Blood - 1).ToString("D2")}", "]",
                        "Name:", NewPlayer.Name, "(", NewPlayer.Uuid.ToString(), ")");
                    PlayersList.Add(NewPlayer);
                    JoinedEvnt?.Invoke(NewPlayer);
                }
                //LifeTimer();
            }
            public void Clear()
            {
                if (PlayersList.Count > 0)
                    PlayersList.Clear();
            }
            public void LifeTimer()
            {
                for (int i = 0; i < PlayersList.Count; i++)
                {
                    PlayersList[i].Blood--;

                    if (PlayersList[i].Blood == 0)
                    {
                        Player PlayerTmp = PlayersList[i];
                        PlayersList.Remove(PlayersList[i--]);
                        //从屏幕上移除这个玩家&修改其它玩家的序号(屏幕上的)
                        Screen.RemoveLine(PlayerTmp.ScreenTag, true);
                        if (PlayersList.Count > 0)
                        {
                            for (int j = 0; j < PlayersList.Count; j++)
                            {
                                Screen.ReviseField((j + 1).ToString("D2"), 1, PlayersList[j].ScreenTag);
                            }
                        }
                        DisconnectedEvent?.Invoke(PlayerTmp);
                    }
                    else
                    {
                        Screen.ReviseField(
                            $"{GetBloodColor(PlayersList[i].Blood, Config.Blood)}{PlayersList[i].Blood.ToString("D2")}", 3, PlayersList[i].ScreenTag);
                    }
                }
            }
            private void RestoreHealthForAllPlayer(int? blood = null)
            {
                foreach (var Player in PlayersList)
                {
                    Player.Blood = blood ?? Config.Blood;
                    Screen.ReviseField($"&a{Player.Blood.ToString("D2")}", 3, Player.ScreenTag);
                }
            }
            private string GetBloodColor(int nowBlood, int maxBlood)
            {
                if (PlayersList.Count <= 12)
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
                foreach (var player in PlayersList)
                {
                    sb.Append($"{player.ToString()}{Environment.NewLine}");
                    sb.Append("---------------------------------");
                }
                return sb.ToString();
            }
            public class Player
            {
                private bool? HasBuyGame = null;
                private bool OnlineMode;
                public Guid Uuid { get; set; }
                public string Name { get; set; }
                public int Blood { get; set; }
                public string ScreenTag { get; set; }

                public Player()
                {

                }
                public Player(string name, Guid uuid, int blood)
                {
                    this.Name = name;
                    this.Uuid = uuid;
                    this.Blood = blood;
                }


                /// <summary>
                /// Need Network
                /// </summary>
                public bool? IsOnlineMode()
                {
                    // GET https://api.mojang.com/users/profiles/minecraft/<username>
                    // 通过这个API获取玩家的UUID,然后和Ping返回的UUID匹配如果不一样的话就是离线模式了
                    if (Uuid != null && !string.IsNullOrWhiteSpace(Name))
                    {
                        //缓存
                        if (HasBuyGame != null && HasBuyGame == true)
                            return OnlineMode;
                        else if (HasBuyGame != null && HasBuyGame != true)
                            return false;
                        //没有缓存的话就去通过API获取
                        try
                        {
                            WebClient wc = new WebClient();
                            string html = Encoding.UTF8.GetString(wc.DownloadData(
                                @"https://api.mojang.com/users/profiles/minecraft/" + Name));
                            HasBuyGame = !string.IsNullOrWhiteSpace(html);
                            if (HasBuyGame != true)
                                return null;
                            OnlineMode = html.Contains(Uuid.ToString().Replace("-", string.Empty));
                            return OnlineMode;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    else
                        throw new ArgumentNullException("Uuid or Name");
                }
                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"PlayerName:{Name}{Environment.NewLine}");
                    sb.Append($"uuid:{Uuid.ToString()}{Environment.NewLine}");
                    sb.Append($"Blood:{Blood}{Environment.NewLine}");
                    sb.Append($"ScreenTag:{ScreenTag}{Environment.NewLine}");
                    return sb.ToString();
                }
            }
        }
        
    }
}
