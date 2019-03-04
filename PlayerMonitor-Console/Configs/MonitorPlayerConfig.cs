using System;
using System.Collections.Generic;
using System.Linq;
using PlayerMonitor.ConsoleOptions;
using PlayerMonitor.ConsolePlus;

namespace PlayerMonitor.Configs
{
    public class MonitorPlayerConfig : ConsoleProgramConfig, IConsoleGuide,IConsoleHelp 

    {
        public override string WindowTitleStyle { get; protected set; } = "$IP:$PORT($PING_TIMEms)";

        public string ServerHost { get; set; }
        public ushort ServerPort { get; set; }

        public int SleepTime { get; set; } = 1600;
        public int Blood { get; set; } = 8;
        public bool AutoSetBlood { get; set; } = false;//未实现
        public string RunCommandForPlayerJoin { get; set; }
        public string RunCommandForPlayerDisconnected { get; set; }

        public MonitorPlayerConfig(string serverHost,ushort serverPort)
        {

        }
        public MonitorPlayerConfig(List<string> argumentList)
        {
            if (argumentList.Any())
            {
                LoadByConsoleOptions(argumentList);
                if (string.IsNullOrWhiteSpace(this.ServerHost) || ServerPort == default(ushort))
                {
                    Console.WriteLine("缺少有效的服务器地址或端口号.");
                    (this as IConsoleGuide).OpenGuide();
                }
            }
            else
                base.LoadByConsoleOptions(argumentList);
        }


        protected override void LoadByConsoleOptions(List<string> argumentList)
        {
            if (argumentList == null)
                throw new ArgumentNullException(nameof(argumentList));

            for (int i = 0; i < argumentList.Count; i++)
            {
                try
                {
                    //这边我有点想改成大小写敏感..
                    switch (argumentList[i].ToLower())
                    {
                        case "-h":
                        case "-help":
                        case "--help":
                            (this as IConsoleHelp).Show();
                            break;
                        case "-i":
                        case "-ip":
                        case "-host":
                            this.ServerHost = argumentList[i + 1];
                            i++;
                            break;
                        case "-p":
                        case "-port":
                            this.ServerPort = ushort.Parse(argumentList[i + 1]);
                            i++;
                            break;
                        case "-s":
                        case "-sleep":
                            this.SleepTime = int.Parse(argumentList[i + 1]);
                            i++;
                            break;
                        case "-b":
                        case "-blood":
                            this.Blood = int.Parse(argumentList[i + 1]);
                            i++;
                            break;
                        case "--color-minecraft":
                            this.SwitchColorScheme(new ConsolePlus.ColorSchemes.MinecraftColorScheme());
                            break;
                        case "--script-logged":
                            this.RunCommandForPlayerJoin = argumentList.Count >= i + 1 ? argumentList[i + 1] : throw new Exception($"option {argumentList[i]} it value is empty");
                            i++;
                            break;
                        case "--script-loggedout":
                            this.RunCommandForPlayerDisconnected = argumentList.Count >= i + 1 ? argumentList[i + 1] : throw new Exception($"option {argumentList[i]} it value is empty");
                            i++;
                            break;

                        default:
                            ColorfulConsole.WriteLine($"&c错误:\r\n &r未知命令行选项:{argumentList[i]}\r\n");
                            Program.Exit(false);
                            break;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (argumentList.Count <= i + 1)
                    {
                        ColorfulConsole.WriteLine($"&c错误:\r\n &r命令行选项 \"&e{argumentList[i]}&r\" 需要一个参数.\r\n");
                        Program.Exit(false);
                    }
                    else
                        throw;
                    
                }
                catch (FormatException)
                {
                    ColorfulConsole.Write($"&c错误:\r\n &r命令行选项 \"&e{argumentList[i]}&r\" 的值无法被转换,");
                    switch (argumentList[i].ToLower())
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
                    Program.Exit(false);
                }
            }
            //这里我当初是怎么想的???
            //if (!string.IsNullOrWhiteSpace(this.ServerHost))
            //    this.ServerPort = Minecraft.DefaultPortOfServer;
            
        }

        bool IConsoleGuide.OpenGuide()
        {
            string InputPrompt_Host = $"服务器地址:";
            string InputPrompt_Port = $"服务器端口号(1-{ushort.MaxValue}):";

            while (string.IsNullOrWhiteSpace(this.ServerHost))
            {
                Console.Write(InputPrompt_Host);
                Console.ForegroundColor = ConsoleColor.White;
                string UserInput = Console.ReadLine();
                //我的解析写的有问题,可能有一些地址是可以用的但是我这边就是匹配不了,所以这边暂时加了一个强制使用符.
                if (UserInput.Length>0&&UserInput[UserInput.Length-1] == '!')
                {
                    this.ServerHost = UserInput;
                }
                else
                {
                    var BaseInfo = Minecraft.ServerAddressResolve(UserInput);
                    if (BaseInfo != null)
                    {
                        this.ServerHost = BaseInfo.Value.Host;
                        this.ServerPort = BaseInfo.Value.Port;
                    }
                    else if (InputPrompt_Host[0] != '你')
                    {
                        InputPrompt_Host = "你输入的不是一个有效的服务器地址,请重新输入:";
                    }
                }
                Console.ResetColor();
            } 
            
            //如果使用了"!"就需要用户补充一下端口号,或者用户使用的是命令行选项来开启程序,但是没使用 "-port" 选项
            while (this.ServerPort==default(ushort))
            {
                Console.Write(InputPrompt_Port);
                Console.ForegroundColor = ConsoleColor.White;
                string UserInput = Console.ReadLine();
                if (ushort.TryParse(UserInput, out ushort port))
                    this.ServerPort = port;
                else if(InputPrompt_Port[0]!='你')
                    InputPrompt_Port = "你输入的不是一个有效的端口号,请重新输入:";
                Console.ResetColor();
            }
            Console.Clear();//要不要清屏这个问题我有点犹豫的,写完再看看要不要清吧.
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
            Console.WriteLine($" -h, -help\t\t\t\t显示帮助信息");
            Console.WriteLine($" -i, -ip IP\t\t\t\t服务器IP地址或域名");
            Console.WriteLine($" -p, -port PORT\t\t\t\t服务器端口号(范围:1-65535),不使用这个命令行选项会使用MC的默认端口号({Minecraft.DefaultPortOfServer})");
            Console.WriteLine($" -s, -sleep TIME\t\t\t每次Ping完服务器后休眠的时间(单位:毫秒,默认:{this.SleepTime}ms)");
            Console.WriteLine($" -b, -blood DEFULT_BLOOD\t\t一个玩家被检测到后的初始血量(默认8)\r\n");//这边的8有问题,如果我改了默认值这边也不会变化,不过我懒的处理这个问题了.
            
            Console.WriteLine(" --script-logged SCRIPT_DIR\t\t当一个玩家加入服务器后会执行这个程序(如果路径中有空格需要在头尾加双引号)");
            Console.WriteLine(" --script-loggedout SCRIPT_DIR\t\t当一个玩家离开服务器后会执行这个程序(如果路径中有空格需要在头尾加双引号)\r\n");
			
            Console.WriteLine(" --color-minecraft \t\t\t使用MC的RGB值(仅支持Windows)\r\n");
            Program.Exit(false);
        }
    }
}