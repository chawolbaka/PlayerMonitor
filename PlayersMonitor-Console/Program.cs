using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using PlayersMonitor.Modes;
using System.Collections.Generic;

namespace PlayersMonitor
{
    
    class Program
    {

        private static Configuration Config;
        private static PlayersManager PlayerManager;
        private static bool IsWindows { get { return RuntimeInformation.IsOSPlatform(OSPlatform.Windows); } }
        //private static readonly string ConfigFilePath = "Config.xml";

        static void Main(string[] args)
        {
            Initializing();
			
            switch (Config.RunningMode)
            {
                case Mode.Type.Chart:
                    throw new NotImplementedException("not support now");
                    //string tag= GetServerTag("Data",Config.ServerHost, Config.ServerPort);
                    //Chart ChartMode = new Chart(Config,$"Data/{tag}");
                    //ChartMode.StartAsync();
                    //break;
                case Mode.Type.Monitor:
                    MonitorPlayer Monitor = new MonitorPlayer(Config, PlayerManager);
                    Monitor.StartAsync();
                    break;
            }

            //Olny Windows Support this
            while (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ConsoleKeyInfo Input = Console.ReadKey(true);
                if (Input.Key == ConsoleKey.Q || Input.Key == ConsoleKey.Escape)
                {
                    Console.CursorVisible = true;
                    Environment.Exit(0);
                }

            }
        }
        private static void Initializing()
        {
            if (!IsWindows)
            {
                //我改成UTF-8好像在一些Windows下会乱码,所以我暂时不改Windows的了
                //(以后添加启动参数更改)
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
            }
            Config = Configuration.Load(Environment.GetCommandLineArgs());
            PlayerManager = new PlayersManager(Config);
            Console.CancelKeyPress += ControlC_Handler;

            //注册玩家上下线的事件
            if (!string.IsNullOrWhiteSpace(Config.RunCommandForPlayerJoin))
            {
                PlayerManager.PlayerJoinedEvnt += player =>
                {
                    string reg = @"^(\S+)( (.*))?$";
                    ProcessStartInfo StartInfo = new ProcessStartInfo();
                    StartInfo.FileName = Regex.Replace(Config.RunCommandForPlayerJoin, reg, "$1");
                    if (Config.RunCommandForPlayerJoin.Contains(" "))
                        StartInfo.Arguments = Regex
                        .Replace(Config.RunCommandForPlayerJoin, reg, "$3")
                        .Replace("$PLAYER_NAME", player.Name); ;
                    Process.Start(StartInfo);
                };
            }
            if (!string.IsNullOrWhiteSpace(Config.RunCommandForPlayerDisconnected))
            {
                PlayerManager.PlayerDisconnectedEvent += player =>
                {
                    string reg = @"^(\S+)( (.*))?$";
                    ProcessStartInfo StartInfo = new ProcessStartInfo();
                    StartInfo.FileName = Regex.Replace(Config.RunCommandForPlayerDisconnected, reg, "$1");
                    if (Config.RunCommandForPlayerDisconnected.Contains(" "))
                        StartInfo.Arguments = Regex
                        .Replace(Config.RunCommandForPlayerDisconnected, reg, "$3")
                        .Replace("$PLAYER_NAME",player.Name);
                    Process.Start(StartInfo);
                };
            }
            //让用户输入缺失的内容(这东西有点碍事,所以只在发布的时候出现吧)
#if (!DEBUG)
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
        }
        private static void ControlC_Handler(object sender, ConsoleCancelEventArgs args)
        {
            try
            {
                if (args.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    //因为在修改控制台中的文字时会暂时隐藏光标
                    //所以有概率在还没有改回来的状态下就被用户按下Ctrl+c然后光标就没了所以这边需要恢复一下
                    Console.CursorVisible = true;
                }
            }
            catch (Exception)
            {
                //不知道为什么我在bash下按会出现异常,所以暂时直接丢掉异常信息吧
#if DEBUG
                throw;
#endif
            }
        }
    }
}
