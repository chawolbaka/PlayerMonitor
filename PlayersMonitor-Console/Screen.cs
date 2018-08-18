using System;
using System.Collections.Generic;
using System.Text;

namespace PlayersMonitor
{
    //无视效率,只要不造成闪烁就可以啦(QAQ我想不无视也不行呀,要是还要考虑效率的话我怕连一个可以用的版本都发不出来了)
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
                int TopStringLength = GetStringLength(TopString[y]);
                WriteAt(newValue, TopStringLength, y);
                //清理多余的文本
                if (!string.IsNullOrWhiteSpace(TopStringValueBuff[y])) //第一次不需要清理
                    WriteWhiteSpaceAt(16, TopStringLength + GetStringLength(newValue), y);//这边长度计算有问题
                TopStringValueBuff[y] = newValue;
            }
        }
        public static string CreateLine(params string[] fields)
        {
            int y = Lines.Count > 0 ? Lines.Count : 0;
            Line NewLine = new Line();
            NewLine.y = y;
            NewLine.Tag = Guid.NewGuid().ToString();
            foreach (var fireld in fields)
            {
                NewLine.Fields.Add(new Line.Field() { Value = fireld });
            }
            //计算3种长度
            for (int i = 0; i < NewLine.Fields.Count; i++)
            {
                NewLine.Fields[i].Length = GetStringLength(NewLine.Fields[i].Value);
                if (i == 0)
                {
                    NewLine.Fields[0].StartLocation = 0;
                    NewLine.Fields[0].EndLocation = NewLine.Fields[0].Length;
                }
                else if (i < NewLine.Fields.Count)
                {
                    NewLine.Fields[i].StartLocation = NewLine.Fields[i - 1].EndLocation;
                    NewLine.Fields[i].EndLocation = NewLine.Fields[i].StartLocation + NewLine.Fields[i].Length;
                }
                else
                    break;
            }
            Lines.Add(NewLine);
            //Print
            foreach (var field in NewLine.Fields) 
            {
                WriteAt(field.Value, field.StartLocation,y);
            }
            Console.WriteLine();
            return NewLine.Tag;
        }
        public static void ReviseLineField(string newValue,int Location, string tag)
        {
            Line FoundLine = Lines.Find(x => x.Tag == tag);
            if (FoundLine == null)
                throw new ArgumentException("tag does not exist", nameof(tag));
            if (FoundLine.Fields[Location].Value == newValue)
                return;
            int NewValueLength = GetStringLength(newValue);
            int OldValueLength = GetStringLength(FoundLine.Fields[Location].Value);
            int y = FoundLine.y;
            //长度相同的话只做替换处理,如果不同的话就要把后面全拆了
            if (NewValueLength==OldValueLength)
            {
                WriteAt(newValue, FoundLine.Fields[Location].StartLocation, FoundLine.y);
                return;
            }
            else
            {
                for (int i = Location; i < FoundLine.Fields.Count; i++)
                {
                    if (FoundLine.Fields[i].Length == i)
                        FoundLine.Fields[i].Length = NewValueLength;
                    else
                        FoundLine.Fields[i].Length = GetStringLength(FoundLine.Fields[i].Value);

                    FoundLine.Fields[i].StartLocation = FoundLine.Fields[i - 1].EndLocation;
                    FoundLine.Fields[i].EndLocation = FoundLine.Fields[i].StartLocation + FoundLine.Fields[i].Length;
                }
                for (int i = Location+1; i < FoundLine.Fields.Count; i++)
                {
                    WriteAt(FoundLine.Fields[i].Value, FoundLine.Fields[i].StartLocation,y);
                }
                WriteAt(newValue, FoundLine.Fields[Location].StartLocation, FoundLine.y);
                WriteWhiteSpaceAt(10, FoundLine.Fields[FoundLine.Fields.Count - 1].EndLocation, y);
            }
            FoundLine.Fields[Location].Value = newValue;

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
        private static int GetStringLength(string s,bool HasColorCodeLength =false)
        {
            int length = HasColorCodeLength == false? ~(GetColorCodeCount(s) * 2) + 1:0;
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
