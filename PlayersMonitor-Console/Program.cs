using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PlayersMonitor.Modes;
using PlayersMonitor.Configs;

namespace PlayersMonitor
{
    
    class Program
    {
        public static readonly string Name = "PlayersMonitor";
        public static readonly string Version = "bata 0.2";

        public static bool UseCompatibilityMode = true;//之后在处理这个先直接全部兼容模式

        //即将废弃(等我想出运行模式存储的地方叫什么名字就删掉你)
        private static Configuration Config = new Configuration();
        private static Mode MainMode;

        static void Main(string[] args)
        {

            //修改CMD/PowerShell/Linux Shell的配置.
            ConsoleInitializing();

            MainMode = CrerteMode(Config.RunningMode, args.ToList());//创建主模式的实例
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
        private static void ConsoleInitializing()
        {
            //在一些Windows下不知道为什么会乱码/字显示不全这样子的问题,只有在非兼容模式下修改编码
            if (!UseCompatibilityMode)
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
            }

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs args) {
                //这边可能需要try一下,以前有一行注释说这里在bash下会报错
                if (args.SpecialKey == ConsoleSpecialKey.ControlC)
                    Program.Exit(false);
            };

        }
        private static Mode CrerteMode(Mode.Type modeType, List<string> args)
        {
            switch (modeType)
            {
                case Mode.Type.Chart:
                    throw new NotImplementedException("not support now");
                    //string tag = GetServerTag("Data", Config.ServerHost, Config.ServerPort);
                    //return null;
                case Mode.Type.Monitor:
                    return new MonitorPlayer(new MonitorPlayerConfig(args));
            }
            throw new Exception("创建模式失败.");
        }
        public static void Exit(bool showExitInfo)
        {
            //因为在修改控制台中的文字时会暂时隐藏光标
            //所以有概率在还没有改回来的状态下就被用户按下Ctrl+c然后光标就没了所以这边需要恢复一下
            Console.CursorVisible = true;
            
            if (showExitInfo)
            {
                Console.WriteLine("按任意键关闭程序...");
                Console.ReadKey();
            }
            Environment.Exit(0);
        }
    }
}
