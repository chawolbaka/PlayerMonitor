using System;
using System.Collections.Generic;
using System.Text;
using MinecraftProtocol.DataType;

namespace PlayersMonitor
{
    internal static class Screen
    {
        private static string[] TopString;

        public static void Initializa(params string[] topString)
        {
            TopString = topString;
            Console.CursorVisible = false;
            foreach (var info in TopString)
            {
                CorlorsPrint(info, true);
            }
        }
        public static void SetTopStringValue(string newValue, int y)
        {
            int TopStringLength = 0;
            foreach (var text in TopString[y])
            {
                int tmp = Encoding.UTF8.GetBytes(text.ToString()).Length;
                switch (tmp)
                {
                    case 3: TopStringLength += 2; break;
                    case 4: TopStringLength += 2; break;
                    default: TopStringLength += tmp; break;
                }
            }
            WriteAt(newValue, TopStringLength, y);
            WriteWhiteSpaceAt(20, TopStringLength + newValue.Length-2, y);
        }
        public static void WriteWhiteSpaceAt(int length,int start_x, int start_y)
        {
            if (length>0)
            {
                StringBuilder WhiteSpace = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    WhiteSpace.Append(" ");
                }
                WriteAt(WhiteSpace.ToString(), start_x, start_y);
            }
        }
        public static void WriteAt(string s, int x, int y)
        {
            int buff_top = Console.CursorTop;
            int buff_left = Console.CursorLeft;
            Console.SetCursorPosition(x, y);
            CorlorsPrint(s);
            Console.CursorTop = buff_top;
            Console.CursorLeft = buff_left;
        }
        public static void CorlorsPrint(string s, bool line = false)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i]=='&')
                {
                    switch (s[i + 1])
                    {
                        case '0': Console.ForegroundColor = ConsoleColor.Black; break; 
                        case '1': Console.ForegroundColor = ConsoleColor.DarkBlue; break;
                        case '2': Console.ForegroundColor = ConsoleColor.DarkGreen; break;
                        case '3': Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                        case '4': Console.ForegroundColor = ConsoleColor.DarkRed; break;
                        case '5': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                        case '6': Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                        case '7': Console.ForegroundColor = ConsoleColor.Gray; break;
                        case '8': Console.ForegroundColor = ConsoleColor.DarkGray; break;
                        case '9': Console.ForegroundColor = ConsoleColor.Blue; break;
                        case 'a': Console.ForegroundColor = ConsoleColor.Green; break;
                        case 'b': Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case 'c': Console.ForegroundColor = ConsoleColor.Red; break;
                        case 'd': Console.ForegroundColor = ConsoleColor.Magenta; break;
                        case 'e': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case 'f': Console.ForegroundColor = ConsoleColor.White; break;
                        case 'r': Console.ResetColor(); break;
                    }
                    i+=2;
                }
                Console.Write(s[i]);
            }
            if (line == true)
                Console.WriteLine();
            Console.ResetColor();
        }
    }
}
