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
        private static List<Line> Lines = new List<Line>();
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
                TopStringLength += GetStringLength(TopString[y]);
                //foreach (var text in TopString[y])
                //{
                //    int tmp = Encoding.UTF8.GetBytes(text.ToString()).Length;
                //    switch (tmp)
                //    {
                //        case 3: TopStringLength += 2; break;
                //        case 4: TopStringLength += 2; break;
                //        default: TopStringLength += tmp; break;
                //    }
                //}
                WriteAt(newValue, TopStringLength, y);
                //清理多余的文本
                if (!string.IsNullOrWhiteSpace(TopStringValueBuff[y])) //第一次不需要清理
                    WriteWhiteSpaceAt(16, TopStringLength + newValue.Length-2, y);//这边长度计算有问题
                TopStringValueBuff[y] = newValue;
            }
        }
        public static string CreateLine(int y ,params string[] fields)
        {
            if (Lines.Count > 0 && Lines.Find(v => v.y == y).y == y)
                throw new ArgumentException("This Line is Existed", nameof(y));
            Line NetLine = new Line();
            NetLine.y = y;
            NetLine.Tag = Guid.NewGuid().ToString();
            //开始计算3种长度


            Lines.Add(NetLine);
            foreach (var field in NetLine.Fields)
            {
                WriteAt(field.Value, y + TopString.Length);
            }
            return NetLine.Tag;
        }
        public static void ReviseLineField(string NewValue,int fieldLocation, string tag)
        {
            Line line = Lines.Find(x => x.Tag == tag);
            if (line.Fields[fieldLocation].Value == NewValue)
                return;
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
            bool HasColorCode = s.Contains('&');
            Console.CursorVisible = false;
            Console.SetCursorPosition(x, y);
            if (HasColorCode == true)
                CorlorsPrint(s);
            else
                Console.Write(s);
            Console.SetCursorPosition(buff_left, buff_top);
            Console.CursorVisible = true;
        }
        public static void WriteAt(string s, int y)
        {
            int buff_top = Console.CursorTop;
            bool HasColorCode = s.Contains('&');
            Console.CursorVisible = false;
            Console.SetCursorPosition(Console.CursorLeft, y);
            if (HasColorCode == true)
                CorlorsPrint(s);
            else
                Console.Write(s);
            Console.SetCursorPosition(Console.CursorLeft, buff_top);
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
                        case '0': Console.ForegroundColor = ConsoleColor.Black; i += 2; break; 
                        case '1': Console.ForegroundColor = ConsoleColor.DarkBlue; i += 2; break;
                        case '2': Console.ForegroundColor = ConsoleColor.DarkGreen; i += 2; break;
                        case '3': Console.ForegroundColor = ConsoleColor.DarkCyan; i += 2; break;
                        case '4': Console.ForegroundColor = ConsoleColor.DarkRed; i += 2; break;
                        case '5': Console.ForegroundColor = ConsoleColor.DarkMagenta; i += 2; break;
                        case '6': Console.ForegroundColor = ConsoleColor.DarkYellow; i += 2; break;
                        case '7': Console.ForegroundColor = ConsoleColor.Gray; i += 2; break;
                        case '8': Console.ForegroundColor = ConsoleColor.DarkGray; i += 2; break;
                        case '9': Console.ForegroundColor = ConsoleColor.Blue; i += 2; break;
                        case 'a': Console.ForegroundColor = ConsoleColor.Green; i += 2; break;
                        case 'b': Console.ForegroundColor = ConsoleColor.Cyan; i += 2; break;
                        case 'c': Console.ForegroundColor = ConsoleColor.Red; i += 2; break;
                        case 'd': Console.ForegroundColor = ConsoleColor.Magenta; i += 2; break;
                        case 'e': Console.ForegroundColor = ConsoleColor.Yellow; i += 2; break;
                        case 'f': Console.ForegroundColor = ConsoleColor.White; i += 2; break;
                        case 'r': Console.ResetColor(); i += 2; break;
                    }
                }
                if (i >= s.Length)
                    break;
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
                        case '0': result += 1; i += 2; break;
                        case '1': result += 1; i += 2; break;
                        case '2': result += 1; i += 2; break;
                        case '3': result += 1; i += 2; break;
                        case '4': result += 1; i += 2; break;
                        case '5': result += 1; i += 2; break;
                        case '6': result += 1; i += 2; break;
                        case '7': result += 1; i += 2; break;
                        case '8': result += 1; i += 2; break;
                        case '9': result += 1; i += 2; break;
                        case 'a': result += 1; i += 2; break;
                        case 'b': result += 1; i += 2; break;
                        case 'c': result += 1; i += 2; break;
                        case 'd': result += 1; i += 2; break;
                        case 'e': result += 1; i += 2; break;
                        case 'f': result += 1; i += 2; break;
                        case 'r': result += 1; i += 2; break;
                    }
                }

            }
            return result;
        }
        private static int GetStringLength(string s)
        {
            int length = 0;
            foreach (var text in s)
            {
                int tmp = Encoding.UTF8.GetBytes(text.ToString()).Length;
                switch (tmp)
                {
                    case 3: length += 2; break;
                    case 4: length += 2; break;
                    default: length += tmp; break;
                }
            }
            return length;

        }
        private class Line
        {
            public int y { get; set; }
            public List<Field> Fields { get; set; } = new List<Field>();
            public string Tag { get; set; }
            public class Field
            {
                public string Value { get; set; }
                public int StartLocation { get; set; }
                public int EndLocation { get; set; }
                public int Length { get; set; }
            }
        }
    }
}
