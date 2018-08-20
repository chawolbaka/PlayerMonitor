using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace PlayersMonitor
{
    
    class Program
    {

        public static readonly Version ProgromVersion = new Version("1.0.0");

        private static Configuration Config;
        private static PlayersManager PlayerManager;
        private static bool IsWindows = false;
        private static readonly string ConfigFilePath = "Config.xml";

        static void Main(string[] args)
        {
            Initializa();

			
			
            Modes.MonitorPlayer Monitor = new Modes.MonitorPlayer(Config,PlayerManager);
            Monitor.StartAsync();

            while (true)
            {
                ConsoleKeyInfo Input = Console.ReadKey(true);
                if (Input.Key == ConsoleKey.Q || Input.Key == ConsoleKey.Escape)
                {
                    Console.CursorVisible = true;
                    Environment.Exit(0);
                }
            }
            
        }
        static void Initializa()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            if (Environment.GetCommandLineArgs().Length == 0)
            {
                //寻找配置文件,如果没找到就启动设置向导,并询问用户是否保存配置
                if(File.Exists(ConfigFilePath))
                {
                    //Config = Configuration.Load(ConfigFilePath);
                }
            }
            else {
                Config = Configuration.Load(Environment.GetCommandLineArgs());
            }
            PlayerManager = new PlayersManager(Config);
            if (!string.IsNullOrWhiteSpace(Config.RunCommandForPlayerJoin))
            {
                PlayerManager.PlayerJoinedEvnt += player =>
                {
                    string reg = @"(\S+)\s(\S+)";
                    ProcessStartInfo StartInfo = new ProcessStartInfo();
                    StartInfo.FileName = Regex.Replace(Config.RunCommandForPlayerJoin, reg, "$1");
                    if (Config.RunCommandForPlayerJoin.Contains(" "))
                        StartInfo.Arguments = Regex.Replace(Config.RunCommandForPlayerJoin, reg, "$2");
                    Process.Start(StartInfo);
                };
            }
            if (!string.IsNullOrWhiteSpace(Config.RunCommandForPlayerDisconnected))
            {
                PlayerManager.PlayerJoinedEvnt += player =>
                {
                    string reg = @"(\S+)\s(\S+)";
                    ProcessStartInfo StartInfo = new ProcessStartInfo();
                    StartInfo.FileName = Regex.Replace(Config.RunCommandForPlayerDisconnected, reg, "$1");
                    if (Config.RunCommandForPlayerDisconnected.Contains(" "))
                        StartInfo.Arguments = Regex.Replace(Config.RunCommandForPlayerDisconnected, reg, "$2");
                    Process.Start(StartInfo);
                };
            }
            //这东西有点碍事,所以只在发布的时候出现吧
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
        }

        
    }
}
