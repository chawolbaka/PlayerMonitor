using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PlayerMonitor.Configs;
using PlayerMonitor.ConsolePlus;
using PlayerMonitor.Modes;

namespace PlayerMonitor
{

    class Program
    {
        public static readonly string Name = "PlayerMonitor";
        public static readonly string Version = "bata 0.3";

        private static Mode MainMode;

        static void Main(string[] args)
        {
            //修改CMD/PowerShell/Linux Shell的配置.
            ConsoleInitializing();
            MainMode = CrerteModeByConsoleOption(args);//创建主模式的实例
            MainMode.StartAsync();

            //Olny Windows Support this
            while (Platform.IsWindows)
            {
                ConsoleKeyInfo Input = Console.ReadKey(true);
                if (Input.Key == ConsoleKey.Q || Input.Key == ConsoleKey.Escape)
                {
                    Console.CursorVisible = true;
                    Environment.Exit(0);
                }
                else if (Input.Key == ConsoleKey.F5)
                    Screen.Refurbih();//bug:隔壁线程在写东西的时候如果用户去按F5会崩溃
            }
        }
        private static void ConsoleInitializing()
        {
            //linux下设置终端标题感觉不太好的样子(虽然后面还是会设置
            if (Platform.IsWindows)
                Console.Title = $"{Program.Name}({Program.Version})";

            //Win10以下的Windows不知道为什么会有乱码或者字显示不全这样子的问题
            if (Platform.IsWindows && Environment.OSVersion.Version.Major >= 10)
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
            }

            //设置默认字体颜色
            //(这行删了也可以正常运行,但是如果在使用ColorfullyConsole前使用Console类设置了字体颜色会导致ColorfullyConsole.ResetColor的颜色不对)
            ColorfullyConsole.Init();
            
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs args) {
                //这边可能需要try一下,以前有一行注释说这里在bash下会报错
                if (args.SpecialKey == ConsoleSpecialKey.ControlC)
                    Program.Exit(false);
            };

            //Linux下窗口变动后会导致显示的位置不正常,而且我还弄不出刷新键
            //所以暂时先加个监视窗口变化的线程来解决这个问题
            if (!Platform.IsWindows)
            {
                Thread HeightWatching = new Thread(() =>
                {
                    int LastHeight = Console.WindowHeight;
                    while (true)
                    {
                        if (Screen.Count > 0)
                        {
                            int CurrentHeight = Console.WindowHeight;
                            if (CurrentHeight != LastHeight)
                            {
                                LastHeight = CurrentHeight;
                                Screen.Refurbih();
                            }
                        }
                        Thread.Sleep(2800);
                    }
                });
                HeightWatching.Name = nameof(HeightWatching);
                HeightWatching.Start();
            }
        }
        private static Mode CrerteModeByConsoleOption(Span<string> args)
        {
            const Mode.Type DefaultMode = Mode.Type.Monitor;
            Mode.Type RunningMode;
            //从命令行选项读取要创建的模式
            if (args.Length > 0 && Enum.TryParse(args[0], out RunningMode))
                args = args.Slice(1);
            else
                RunningMode = DefaultMode;
            //创建模式
            switch (RunningMode)
            {
                case Mode.Type.Chart:
                    throw new NotImplementedException("Chart模式未完成,暂时无法使用(咕咕咕)");
                case Mode.Type.Monitor:
                    return new MonitorPlayer(new MonitorPlayerConfig(args));
                default: throw new NotSupportedException($"模式{RunningMode}不存在");
            }
            throw new Exception("创建模式失败.");
        }
        public static void Exit(string info,bool hasPause,int exitCode)
        {
            //因为在修改控制台中的文字时会暂时隐藏光标
            //所以有概率在还没有改回来的状态下就被用户按下Ctrl+c然后光标就没了所以这边需要恢复一下
            Console.CursorVisible = true;
            if (!string.IsNullOrEmpty(info))
                ColorfullyConsole.WriteLine(info);
            if (hasPause)
            {
                Console.WriteLine("按任意键关闭程序...");
                Console.ReadKey();
            }
            Environment.Exit(exitCode);
        }
        public static void Exit() => Exit(string.Empty, Platform.IsWindows, 0);
        public static void Exit(string info) => Exit(info, Platform.IsWindows, 0);
        public static void Exit(bool hasPause) => Exit(string.Empty, hasPause, 0);
        public static void Exit(int exitCode) => Exit(string.Empty, Platform.IsWindows, exitCode);
        public static void Exit(bool hasPause, int exitCode) => Exit(string.Empty, hasPause, exitCode);
        public static void Exit(string info, int exitCode) => Exit(info, Platform.IsWindows, exitCode);
    }
}
