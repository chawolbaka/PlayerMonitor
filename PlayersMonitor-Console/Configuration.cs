using System;
using System.Text.RegularExpressions;

namespace PlayersMonitor
{
    public class Configuration
    {
        public string ServerHost { get; set; }
        public ushort ServerPort { get; set; }

        public int SleepTime { get; set; } = 1500;
        public int Blood { get; set; } = 8;
        public bool AutoSetBlood { get; set; } = false;


        public static Configuration Load(string[] args)
        {
            Configuration config = new Configuration();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-ip" && args.Length >= i + 1)
                    config.ServerHost = Regex.Replace(args[i + 1], @"\s", "");
                else if (args[i] == "-p" || args[i] == "-port" && args.Length >= i + 1)
                {
                    if (ushort.TryParse(args[i + 1], out ushort tmp) == false)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-p (1-65535)");
                        System.Environment.Exit(0);
                    }
                    else
                        config.ServerPort = tmp;
                }
                else if (args[i] == "-b" || args[i] == "-blood" && args.Length >= i + 1)
                {
                    if (args[i] + 1 == "auto")
                    {
                        config.AutoSetBlood = true;
                        continue;
                    }
                    else if (int.TryParse(args[i + 1], out int tmp) == false && tmp >= 0)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-b (0-{int.MaxValue})");
                        System.Environment.Exit(0);
                    }
                    else
                        config.Blood = tmp;
                }
                else if (args[i] == "-s" || args[i] == "-sleep" && args.Length >= i + 1)
                {
                    if (int.TryParse(args[i + 1], out int tmp) == false&&tmp>=0)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-s (0-{int.MaxValue})");
                        System.Environment.Exit(0);
                    }
                    else
                        config.SleepTime= tmp;
                }
            }
            return config;
        }

        public static Configuration Load(string ConfigFilePath)
        {
            throw new NotImplementedException();
        }
    }

}