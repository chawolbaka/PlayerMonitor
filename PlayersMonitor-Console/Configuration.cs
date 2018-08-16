using System;
using System.Text.RegularExpressions;

namespace PlayersMonitor_Console
{
    public class Configuration
    {
        public string ServerHost { get; set; }
        public ushort ServerPort { get; set; }

        public uint SleepTime { get; set; } = 1500;
        public uint Blood { get; set; }
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
                    else if (uint.TryParse(args[i + 1], out uint tmp) == false)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-b (0-{uint.MaxValue})");
                        System.Environment.Exit(0);
                    }
                    else
                        config.Blood = tmp;
                }
                else if (args[i] == "-s" || args[i] == "-sleep" && args.Length >= i + 1)
                {
                    if (uint.TryParse(args[i + 1], out uint tmp) == false)
                    {
                        Console.WriteLine($"参数错误{Environment.NewLine}Use:-s (0-{uint.MaxValue})");
                        System.Environment.Exit(0);
                    }
                    else
                        config.Blood = tmp;
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