using System;
using System.Text.RegularExpressions;
using PlayersMonitor.Modes;

namespace PlayersMonitor
{
    public class Configuration
    {

        public string TitleStyle { get; } = "$IP:$PORT($PING_TIMEms)";
        public Mode.Type RunningMode { get; } = Mode.Type.Monitor;
        public string ServerHost { get; set; }
        public ushort ServerPort { get; set; }
        

        public int SleepTime { get; set; } = 1500;
        public int Blood { get; set; } = 8;
        public bool AutoSetBlood { get; set; } = false;
        public string RunCommandForPlayerJoin { get; } =null;
        public string RunCommandForPlayerDisconnected { get; } = null;

        public Configuration()
        {

        }
        public Configuration(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-ip" && args.Length >= i + 1)
                    ServerHost = Regex.Replace(args[i + 1], @"\s", "");
                else if (args[i] == "-p" || args[i] == "-port" && args.Length >= i + 1)
                {
                    if (ushort.TryParse(args[i + 1], out ushort tmp) == false)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-p (1-65535)");
                        Environment.Exit(0);
                    }
                    else
                        ServerPort = tmp;
                }
                else if (args[i] == "-b" || args[i] == "-blood" && args.Length >= i + 1)
                {
                    if (args[i] + 1 == "auto")
                    {
                        AutoSetBlood = true;
                        continue;
                    }
                    else if (int.TryParse(args[i + 1], out int tmp) == false && tmp >= 0)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-b (0-{int.MaxValue})");
                        Environment.Exit(0);
                    }
                    else
                        Blood = tmp;
                }
                else if (args[i] == "-s" || args[i] == "-sleep" && args.Length >= i + 1)
                {
                    if (int.TryParse(args[i + 1], out int tmp) == false && tmp >= 0)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-s (0-{int.MaxValue})");
                        Environment.Exit(0);
                    }
                    else
                        SleepTime = tmp;
                }
                else if (args[i] == "--title-style" && args.Length >= i + 1)
                {
                    TitleStyle = args[i + 1];
                }
                else if (args[i] == "-r"|| args[i].ToLower() == "--script-logged" && args.Length >= i + 1)
                {
                    RunCommandForPlayerJoin = args[i + 1];
                }
                else if (args[i].ToLower() == "--script-loggedout" && args.Length >= i + 1)
                {
                    RunCommandForPlayerDisconnected = args[i + 1];
                }
                else if (args[i].ToLower() == "-mode" && args.Length >= i + 1)
                {
                    switch (args[i+1].ToLower())
                    {
                        case "monitor": RunningMode = Mode.Type.Monitor; break;
                        case "chart": RunningMode = Mode.Type.Chart; break;
                        default://我直接抛异常是不是不太好,不过现在懒的想怎么提醒用户了
                            Console.WriteLine("QAQ不要输入不存在的模式呀...");
                            throw new ArgumentOutOfRangeException(
                                "-mode",//Argument Name
                                "monitor or chart",//ActualValue(这是允许范围吗?)
                                "input argument out of support range");//Message
                    }
                }
            }
        }
        public static Configuration Load(string[] args) => new Configuration(args);
        public static Configuration Load(string ConfigFilePath)
        {
            throw new NotImplementedException();
        }
    }

}