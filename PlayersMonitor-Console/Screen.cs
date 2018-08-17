using System;
using System.Collections.Generic;
using System.Text;

namespace PlayersMonitor
{
    //无视效率,只要不造成闪烁就可以啦
    internal static class Screen
    {
        private static string[] TopString;
        private static string[] TopStringValueBuff;

        public static void Initializa(params string[] topString)
        {
            TopString = topString;
            TopStringValueBuff = new string[TopString.Length];
            foreach (var info in TopString)
            {
                CorlorsPrint(info, true);
            }
        }
        public static void SetTopStringValue(string newValue, int y)
        {
            if (y > TopString.Length)
                throw new ArgumentOutOfRangeException("y",y,
                    $"\"y\" out of initialization range(initialization set:{TopString.Length})");
            if (TopStringValueBuff[y] != newValue && !string.IsNullOrWhiteSpace(newValue))
            {
                int HeadStringColorCodeCount = ~(GetColorCodeCount(TopString[y]) * 2) + 1;
                int TopStringLength = HeadStringColorCodeCount;
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
                //清理多余的文本
                if (!string.IsNullOrWhiteSpace(TopStringValueBuff[y])) //第一次不需要清理
                    WriteWhiteSpaceAt(16, TopStringLength + newValue.Length-2, y);//这边长度计算有问题
                TopStringValueBuff[y] = newValue;
            }
        }
        public static void WriteWhiteSpaceAt(int length,int start_x, int start_y)
        {
            if (length > 0)
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

            Console.CursorVisible = false;
            Console.SetCursorPosition(x, y);
            CorlorsPrint(s);
            Console.SetCursorPosition(buff_top, buff_left);
            Console.CursorVisible = true;
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
        public static int GetColorCodeCount(string s)
        {
            int result = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '&')
                {
                    switch (s[i + 1])
                    {
                        case '0': result += 1; break;
                        case '1': result += 1; break;
                        case '2': result += 1; break;
                        case '3': result += 1; break;
                        case '4': result += 1; break;
                        case '5': result += 1; break;
                        case '6': result += 1; break;
                        case '7': result += 1; break;
                        case '8': result += 1; break;
                        case '9': result += 1; break;
                        case 'a': result += 1; break;
                        case 'b': result += 1; break;
                        case 'c': result += 1; break;
                        case 'd': result += 1; break;
                        case 'e': result += 1; break;
                        case 'f': result += 1; break;
                        case 'r': result += 1; break;
                    }
                    i += 2;
                }

            }
            return result;
        }

    }
}
