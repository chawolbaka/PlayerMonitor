using System;
using System.Text;
using System.Collections.Generic;
using MinecraftProtocol.Utils;
using System.Text.RegularExpressions;
using System.Threading;

namespace PlayersMonitor
{
    
    class Program
    {
        public static Configuration Config;
        static void Main(string[] args)
        {
            Initialization();
            //Test Server
            Config.ServerHost = "dx.g.mcmiao.com";
            Config.ServerPort = 37554;

            Ping ping = new Ping(Config.ServerHost, Config.ServerPort);
            FirstPrint(ping);
            while (true)
            {
                var PingResult = ping.Send();
                Console.Title = Settings.TitleStyle.
                    Replace("$IP", Config.ServerHost).
                    Replace("$PORT", Config.ServerPort.ToString()).
                    Replace("$PING_TIME", ((float)(PingResult.Time / 10000.0f)).ToString("F2"));
                Screen.SetTopStringValue($"&a{PingResult.Version.Name}", 0);
                Screen.SetTopStringValue($"&f{PingResult.Player.Online}/{PingResult.Player.Max}", 1);
                Thread.Sleep(Config.SleepTime);
            }

        }
        static void FirstPrint(Ping ping)
        {
            //区别:可以一下子就把信息显示出来,降低延迟感
            var PingResult = ping.Send();
            Console.Title = Settings.TitleStyle.
                Replace("$IP", Config.ServerHost).
                Replace("$PORT", Config.ServerPort.ToString()).
                Replace("$PING_TIME", ((float)(PingResult.Time / 10000.0f)).ToString("F2"));
            Screen.Initializa("服务端版本:", "在线人数:");
            Screen.SetTopStringValue($"&a{PingResult.Version.Name}", 0);
            Screen.SetTopStringValue($"&f{PingResult.Player.Online}/{PingResult.Player.Max}", 1);
            Thread.Sleep(Config.SleepTime);
        }
        static void Initialization()
        {
            Config = Configuration.Load(Environment.GetCommandLineArgs());
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            #if (DEBUG==false)
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
    }
}
