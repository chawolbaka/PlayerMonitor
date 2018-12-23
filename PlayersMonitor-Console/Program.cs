﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using PlayersMonitor.Modes;

namespace PlayersMonitor
{
    
    class Program
    {
        public static bool UseCompatibilityMode = true;//之后在处理这个先直接全部兼容模式

        private static Configuration Config;
        private static Mode MainMode;

        static void Main(string[] args)
        {
            Screen.SetColorScheme(new ConsoleRewrite.ColorSchemes.MinecraftColorScheme());

            Initializing();//初始化启动参数
            MainMode = CrerteMode(Config.RunningMode);//创建主模式的实例
            MainMode.StartAsync();


            //Olny Windows Support this
            while (SystemInfo.IsWindows)
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
            //在一些Windows下不知道为什么会乱码/字显示不全这样子的问题,只有在非兼容模式下修改编码
            if (!UseCompatibilityMode)
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
            }

            Config = Configuration.Load(Environment.GetCommandLineArgs());

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs args) {
                //这边可能需要try一下,以前有一行注释说这里在bash下会报错
                if (args.SpecialKey == ConsoleSpecialKey.ControlC)
                    Exit();
            };
            
            //让用户输入缺失的内容(这东西有点碍事,所以只在发布的时候出现吧)
#if !DEBUG
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
        private static Mode CrerteMode(Mode.Type modeType)
        {
            switch (modeType)
            {
                case Mode.Type.Chart:
                    throw new NotImplementedException("not support now");
                    //string tag = GetServerTag("Data", Config.ServerHost, Config.ServerPort);
                    //return null;
                case Mode.Type.Monitor:
                    return new MonitorPlayer(Config);
                default: return null;
            }
        }
        private static void Exit()
        {
            //因为在修改控制台中的文字时会暂时隐藏光标
            //所以有概率在还没有改回来的状态下就被用户按下Ctrl+c然后光标就没了所以这边需要恢复一下
            Console.CursorVisible = true;
            Environment.Exit(0);
        }
    }
}
