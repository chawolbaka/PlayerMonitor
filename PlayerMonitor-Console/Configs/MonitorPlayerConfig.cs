using System;
using System.Collections.Generic;
using System.Linq;
using PlayerMonitor.ConsoleOptions;
using PlayerMonitor.ConsolePlus;
using PlayerMonitor.Modes;

namespace PlayerMonitor.Configs
{
    public class MonitorPlayerConfig : ConsoleProgramConfig, IConsoleGuide,IConsoleHelp
    { 
        public override string WindowTitleStyle { get; protected set; } = "$IP:$PORT($PING_TIMEms)";

        public string ServerHost { get; set; }
        public ushort ServerPort { get; set; }

        public int SleepTime { get; set; } = 1600;
        public int Blood { get; set; } = 8;
        public List<string> HighlightList { get; set; } = new List<string>();
        public string HighlightColor { get; set; } = "&c";
        public bool AutoSetBlood { get; set; } = false;//未实现
        public string RunCommandForPlayerJoin { get; set; }
        public string RunCommandForPlayerDisconnected { get; set; }

        public MonitorPlayerConfig(ReadOnlySpan<string> args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length > 0)
            {
                LoadByConsoleOptions(args);
                if (this.ServerPort == default(ushort))
                    this.ServerPort = Minecraft.DefaultPortOfServer;
                if (string.IsNullOrWhiteSpace(this.ServerHost))
                {
                    Console.WriteLine("缺少有效的服务器地址.");
                    (this as IConsoleGuide).OpenGuide();
                }
            }
            else
                base.LoadByConsoleOptions(args);
        }

        protected override void LoadByConsoleOptions(ReadOnlySpan<string> args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    //这边我有点想改成大小写敏感..
                    switch (args[i].ToLower())
                    {
                        case "-h":
                        case "-help":
                        case "--help":
                            (this as IConsoleHelp).Show(); break;
                        case "-i":
                        case "-ip":
                        case "-host":
                            this.ServerHost = args[++i]; break;
                        case "-p":
                        case "-port":
                            this.ServerPort = ushort.Parse(args[++i]); break;
                        case "-s":
                        case "-sleep":
                            this.SleepTime = int.Parse(args[++i]); break;
                        case "-b":
                        case "-blood":
                            this.Blood = int.Parse(args[++i]); break;
                        case "--highlight":
                            this.HighlightList.AddRange(args[++i].Replace('，',',').Split(',')); break;
                        case "--highlight-color":
                            this.HighlightColor = GetColorCode(args[++i]); break;
                        case "--color-minecraft":
                            this.SwitchColorScheme(new ConsolePlus.ColorSchemes.MinecraftColorScheme()); break;
                        case "--watchcat":
                            new Watchcat().Start(1000 * 26, 8, 20); break;
                        case "--script-logged":
                            this.RunCommandForPlayerJoin = args.Length >= i + 1 ? args[++i] : throw new Exception($"option {args[i]} it value is empty"); break;
                        case "--script-loggedout":
                            this.RunCommandForPlayerDisconnected = args.Length >= i + 1 ? args[++i] : throw new Exception($"option {args[i]} it value is empty");break;
                        default:
                            ColorfullyConsole.WriteLine($"&c错误:\r\n &r未知命令行选项:{args[i]}\r\n");
                            Program.Exit(false,-1);
                            break;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (args.Length <= i + 1)
                    {
                        ColorfullyConsole.WriteLine($"&c错误:\r\n &r命令行选项 \"&e{args[i]}&r\" 需要一个参数.\r\n");
                        Program.Exit(false,-1);
                    }
                    else
                        throw;
                    
                }
                catch (FormatException)
                {
                    ColorfullyConsole.Write($"&c错误:\r\n &r命令行选项 \"&e{args[i]}&r\" 的值无法被转换,");
                    switch (args[i].ToLower())
                    {
                        case "-p":
                        case "-port":
                            Console.WriteLine("请输入一个有效的端口号(1-65535)");
                            break;
                        case "-s":
                        case "-b":
                        case "-sleep":
                        case "-blood":
                            Console.WriteLine("它不是一个有效的32位带符号整数.");
                            break;
                        default:
                            Console.WriteLine("超出范围.");
                            break;
                    }
                    Console.WriteLine();
                    Program.Exit(false,-1);
                }
            }
            //这里我当初是怎么想的???
            //if (!string.IsNullOrWhiteSpace(this.ServerHost))
            //    this.ServerPort = Minecraft.DefaultPortOfServer;
            
        }
        string GetColorCode(string arg)
        {
            if(string.IsNullOrWhiteSpace(arg))
            {
                ColorfullyConsole.Write($"&c错误: \r\n &r选项 \"&e--highlight-color&r\" 没有值");
                Program.Exit(false,-1); return "";
            }

            try
            {
                int ColorCode = Convert.ToInt32(arg, 16);
                if (ColorCode >= 0 && ColorCode <= 0xf)
                    return ColorfullyConsole.DefaultColorCodeMark + ColorCode.ToString("x");
            }
            catch (FormatException)
            {
                string ColorText = arg.ToLower();
                foreach (var name in typeof(ConsoleColor).GetEnumNames())
                {
                    if (name.ToLower()==ColorText&&Enum.TryParse(name, out ConsoleColor color))
                        return ColorfullyConsole.DefaultColorCodeMark + ((int)color).ToString("x");
                }
            }

            ColorfullyConsole.WriteLine($"&c错误&r: 颜色 \"{arg}\" 不存在,可用的颜色:");
            foreach (var name in typeof(ConsoleColor).GetEnumNames())
            {
                ConsoleColor CurrentColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), name);
                ColorfullyConsole.WriteLine($"0x{((int)CurrentColor).ToString("x")}: {name}", CurrentColor);
            }
            Program.Exit(false,-1); return "";
        }

        bool IConsoleGuide.OpenGuide()
        {
            string InputPrompt_Host = "服务器地址:";
            Random egg = new Random();

            while (string.IsNullOrWhiteSpace(this.ServerHost))
            {
                if (egg.Next(0, 233) == 23)
                    ColorfullyConsole.WriteRainbow(InputPrompt_Host);
                else
                    ColorfullyConsole.Write(InputPrompt_Host, ConsoleColor.White);
                string UserInput = Console.ReadLine();
                //我的解析写的有问题,可能有一些地址是可以用的但是我这边就是匹配不了,所以这边暂时加了一个强制使用符.
                if (UserInput.Length>0&&UserInput[UserInput.Length-1] == '!')
                {
                    this.ServerHost = UserInput;
                }
                else
                {
                    var BaseInfo = Minecraft.ServerAddressResolve(UserInput);
                    if (BaseInfo.HasValue)
                    {
                        this.ServerHost = BaseInfo.Value.Host;
                        this.ServerPort = BaseInfo.Value.Port;
                    }
                    else if (InputPrompt_Host[0] != '你')
                    {
                        InputPrompt_Host = "你输入的不是一个有效的服务器地址,请重新输入:";
                    }
                }
            }
            return true;
        }

        void IConsoleHelp.Show()
        {
            if (Platform.IsWindows)
            {
                Console.WriteLine($"Usege: {Program.Name}.exe 模式 [选项]");
                Console.WriteLine($"       {Program.Name}.exe [选项]\r\n");
            }
            else
            {
                Console.WriteLine($"Usege: {Program.Name} Mode [Options]");
                Console.WriteLine($"       {Program.Name} [Options]");
                //Console.WriteLine($"       dotnet {Program.Name}.dll Mode [Options]\n");
                //Console.WriteLine($"       dotnet {Program.Name}.dll [Options]");
            }


            Console.WriteLine("Options:");
            //为了在命令行选项里面严格一点,暂时不启用这个功能了.(只在无任何命令行选项的情况启用)
            //Console.WriteLine("-host \t服务器地址:端口号,端口号留空的情况会使用MC默认的25565");
            Console.WriteLine($" -h, -help\t\t\t显示帮助信息");
            Console.WriteLine($" -i, -ip\t\t\t服务器IP地址或域名");
            Console.WriteLine($" -p, -port\t\t\t服务器端口号(范围:1-65535),不指定会使用MC的默认端口号({Minecraft.DefaultPortOfServer})");
            Console.WriteLine($" -s, -sleep\t\t\t每次Ping完服务器后休眠的时间(单位:毫秒,默认:{0}ms)");
            Console.WriteLine($" -b, -blood\t\t\t一个玩家被检测到后的初始血量(默认8)\r\n");//这边的8有问题,如果我改了默认值这边也不会变化,不过我懒的处理这个问题了.

            Console.WriteLine(" --highlight\t\t\t高亮指定的玩家,格式:Player1,player2,player3");
            Console.WriteLine(" --highlight-color\t\t高亮的颜色,默认红色\r\n");

            if (Platform.IsWindows)
            {
                Console.WriteLine(" --script-logged\t\t当一个玩家加入服务器后会被执行(如果路径中有空格需要在头尾加双引号)");
                Console.WriteLine(" --script-loggedout\t\t当一个玩家离开服务器后会被执行(如果路径中有空格需要在头尾加双引号)\r\n");

                Console.WriteLine(" --color-minecraft \t\t使用MC的RGB值(仅支持Windows)");
            }
            else
            {
                Console.WriteLine(" --script-logged\t\t当一个玩家加入服务器后会被执行");
                Console.WriteLine(" --script-loggedout\t\t当一个玩家离开服务器后会被执行");
            }
            Console.WriteLine(" --watchcat\t\t\t监视CPU使用率,如果连续8秒高于20%就自杀\r\n");
            Program.Exit(false, -1);
        }
    }
}