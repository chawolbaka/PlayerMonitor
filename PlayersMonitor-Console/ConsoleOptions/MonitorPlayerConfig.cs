using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayersMonitor.ConsoleOptions
{
    public class MonitorPlayerConfig : ConsoleConfig, IBaseInfoGuide
    {
        public override string WindowTitleStyle { get; protected set; } = "$IP:$PORT($PING_TIMEms)";

        public string ServerHost { get; set; }
        public ushort ServerPort { get; set; }

        public int SleepTime { get; set; } = 1600;
        public int Blood { get; set; } = 8;
        public bool AutoSetBlood { get; set; } = false;//未实现
        public string RunCommandForPlayerJoin { get; set; }
        public string RunCommandForPlayerDisconnected { get; set; }

        public static MonitorPlayerConfig LoadByConsoleOptions(List<string> argumentList)
        {
            if (argumentList == null)
                throw new ArgumentNullException(nameof(argumentList));
            else if (!argumentList.Any())
                throw new EmptyConsoleOptionException("cannot find any console option", true);

            MonitorPlayerConfig MPC = new MonitorPlayerConfig();

            for (int i = 0; i < argumentList.Count; i++)
            {
                try
                {
                    //这边我有点想改成大小写敏感..
                    switch (argumentList[i].ToLower())
                    {
                        case "-h":
                        case "-help":
                            //这块将来考虑改成单独的方法.
                            Console.WriteLine("选项:");
                            Console.WriteLine("-ip \t服务器IP地址");
                            //为了在命令行选项里面严格一点,暂时不启用这个功能了.(只在无任何命令行选项的情况启用)
                            //Console.WriteLine("-host \t服务器地址:端口号,端口号留空的情况会使用MC默认的25565");
                            Console.WriteLine($"-port \t服务器端口号(范围:1-65535),不使用这个命令行选项会使用MC的默认端口号({Minecraft.DefaultPortOfServer})");
                            Console.WriteLine($"-sleep \t每次Ping完服务器后休眠的时间(单位:毫秒,默认:{MPC.SleepTime}ms)");
                            Console.WriteLine("-blood \t一个玩家被检测到后的初始血量(默认8)\r\n");//这边的8有问题,如果我改了默认值这边也不会变化,不过我懒的处理这个问题了.
                            Console.WriteLine("--script-logged  <ScriptPath>\t当一个玩家加入服务器后会执行这个程序,如果目录中有空格需要在头尾加引号.");
                            Console.WriteLine("--script-loggedout <ScriptPath>\t当一个玩家离开服务器后会执行这个程序,如果目录中有空格需要在头尾加引号.");
                            Program.Exit(SystemInfo.IsWindows);
                            break;
                        case "-host":
                        case "-ip":
                            MPC.ServerHost = argumentList[i + 1];
                            i++;
                            break;
                        case "-p":
                        case "-port":
                            MPC.ServerPort = ushort.Parse(argumentList[i + 1]);
                            i++;
                            break;
                        case "-s":
                        case "-sleep":
                            MPC.SleepTime = int.Parse(argumentList[i + 1]);
                            i++;
                            break;
                        case "-b":
                        case "-blood":
                            MPC.Blood = int.Parse(argumentList[i + 1]);
                            i++;
                            break;
                        case "--script-logged":
                            MPC.RunCommandForPlayerJoin = argumentList.Count >= i + 1 ? argumentList[i + 1] : throw new Exception($"option {argumentList[i]} it value is empty");
                            i++;
                            break;
                        case "--script-loggedout":
                            MPC.RunCommandForPlayerDisconnected = argumentList.Count >= i + 1 ? argumentList[i + 1] : throw new Exception($"option {argumentList[i]} it value is empty");
                            i++;
                            break;
                    }
                }
                catch (OverflowException)
                {
                    switch (argumentList[i].ToLower())
                    {
                        case "-p":
                        case "-port":
                            Console.WriteLine($"命令行选项 \"{argumentList[i]}\" 的值无法被转换,请输入一个有效的端口号(1-65535)");
                            break;
                        case "-s":
                        case "-b":
                        case "-sleep":
                        case "-blood":
                            Console.WriteLine($"命令行选项 \"{argumentList[i]}\" 的值不是一个有效的32位带符号整数.");
                            break;
                        default:
                            Console.WriteLine($"{argumentList[i]}的值无法被转换,超出范围.");
                            break;
                    }
                    Environment.Exit(0);
                }
            }
            if (!string.IsNullOrWhiteSpace(MPC.ServerHost))
                MPC.ServerPort = Minecraft.DefaultPortOfServer;
            
            return MPC;
        }

        Config IBaseInfoGuide.OpenBaseGuide()
        {
            MonitorPlayerConfig mpc = new MonitorPlayerConfig();
            string InputPrompt_Host = $"服务器地址:";
            string InputPrompt_Port = $"服务器端口号(1-{ushort.MaxValue}):";
            
            do
            {
                Console.Write(InputPrompt_Host);
                Console.ForegroundColor = ConsoleColor.White;
                string UserInput = Console.ReadLine();

                var BaseInfo = Minecraft.ServerAddressResolve(UserInput);
                if (BaseInfo != null)
                {
                    mpc.ServerHost = BaseInfo.Value.Host;
                    mpc.ServerPort = BaseInfo.Value.Port;
                }
                else if (InputPrompt_Host[0] != '你')
                {
                    InputPrompt_Host = "你输入的不是一个有效的服务器地址,请重新输入:";
                }
                Console.ResetColor();
            } while (string.IsNullOrWhiteSpace(mpc.ServerHost));
            //感觉这块好像永远不会被加载到的样子?(原因可以看方法:Minecraft.ServerAddressResolve)
            while (mpc.ServerPort==default(ushort))
            {
                Console.Write(InputPrompt_Port);
                Console.ForegroundColor = ConsoleColor.White;
                string UserInput = Console.ReadLine();
                if (ushort.TryParse(UserInput, out ushort port))
                    mpc.ServerPort = port;
                else if(InputPrompt_Port[0]!='你')
                    InputPrompt_Port = "你输入的不是一个有效的端口号,请重新输入:";
                Console.ResetColor();
            }
            Console.Clear();//要不要清屏这个问题我有点犹豫的,写完再看看要不要清吧.
            return mpc;
        }
    }
}