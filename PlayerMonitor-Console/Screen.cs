using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace PlayerMonitor
{
    //无视效率,只要不造成闪烁就可以啦
    //(QAQ我想不无视也不行呀,要是还要考虑效率的话我怕连一个可以用的版本都发不出来了)
    internal static class Screen
    {
        private static Dictionary<Guid,Line> Lines = new Dictionary<Guid, Line>();

        public static int SetDefaultForegroundColor(Color foregroundColor)
        {
            if (!Platform.IsWindows)
                throw new PlatformNotSupportedException("it need WinAPI");
            else
                return WinAPI.ReplaceConsoleColor(ConsoleColor.Gray, foregroundColor.R, foregroundColor.G, foregroundColor.B);
        }
        public static int SetDefaultBackgroundColor(Color backgroundColor)
        {
            if (!Platform.IsWindows)
                throw new PlatformNotSupportedException("it need WinAPI");
            else
                return WinAPI.ReplaceConsoleColor(ConsoleColor.Black, backgroundColor.R, backgroundColor.G, backgroundColor.B);
        }

        public static Guid CreateLine(params string[] fields)
        {
            //添加行(仅集合内)
            Line NewLine = new Line();
            NewLine.y = Lines.Count > 0 ? Lines.Count : 0; 
            Guid Tag = Guid.NewGuid();
            foreach (var fireld in fields)
            {
                NewLine.Fields.Add(new Line.Field() { Value = fireld });
            }            
            //计算每个字段的起始位置和结束位置
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
            Lines.Add(Tag,NewLine);
            //把刚刚添加的行输出到控制台
            foreach (var field in NewLine.Fields) 
            {
                WriteAt(field.Value, field.StartLocation,NewLine.y);
            }
            Console.WriteLine();
            return Tag;
        }
        public static void ReviseField(string newValue,int location, Guid tag)
        {
            Line destLine = Lines[tag];

            if (destLine.Fields[location].Value == newValue)
                return;

            int NewValueLength = GetStringLength(newValue);
            int OldValueLength = GetStringLength(destLine.Fields[location].Value);
            int y = destLine.y;
            //长度相同的话只做替换处理,如果不同的话就要重新计算长度然后把后面的全拆了QAQ
            if (NewValueLength == OldValueLength)
            {
                WriteAt(newValue, destLine.Fields[location].StartLocation, destLine.y);
            }
            else
            {
                //重新计算长度
                for (int i = location; i < destLine.Fields.Count; i++)
                {
                    if (i == location)
                        destLine.Fields[i].Length = NewValueLength;
                    else
                        destLine.Fields[i].Length = GetStringLength(destLine.Fields[i].Value);

                    destLine.Fields[i].StartLocation = destLine.Fields[i - 1].EndLocation;
                    destLine.Fields[i].EndLocation = destLine.Fields[i].StartLocation + destLine.Fields[i].Length;
                }
                //把新的内容输出到控制台上
                WriteAt(newValue, destLine.Fields[location].StartLocation, destLine.y);
                //重新输出一下后面的那些字段
                for (int i = location + 1; i < destLine.Fields.Count; i++)
                {
                    WriteAt(destLine.Fields[i].Value, destLine.Fields[i].StartLocation, y);
                }
                //清理历史残留
                int ClearLength = Console.BufferWidth - destLine.Fields[destLine.Fields.Count - 1].EndLocation;
                WriteWhiteSpaceAt(ClearLength, destLine.Fields[destLine.Fields.Count - 1].EndLocation, y);
            }
            destLine.Fields[location].Value = newValue;
        }
        public static void RemoveLine(Guid tag,bool rePirint=false)
        {
            Line removeLine = Lines[tag];
            Lines.Remove(tag);
            int ClearLength = Console.BufferWidth;

            if (Lines.Count==removeLine.y)
            {
                WriteWhiteSpaceAt(ClearLength, 0, removeLine.y);
            }
            else
            {
                foreach (var line in Lines.Values)
                {
                    if (line.y>=removeLine.y)
                    {
                        line.y--;
                        if (rePirint)
                        {
                            WriteWhiteSpaceAt(ClearLength, 0, line.y);
                            WriteWhiteSpaceAt(ClearLength, 0, line.y + 1);
                            foreach (var Field in line.Fields)
                            {
                                WriteAt(Field.Value, Field.StartLocation, line.y);
                            }
                        }
                    }
                }
            }
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop-1);
        }
        public static bool HasLine(Guid tag)
        {
            return Lines.ContainsKey(tag);
        }
        public static void Clear()
        {
            Lines.Clear();
            Console.Clear();
        }

        public static void Write(string s) => CorlorsPrint(s, false);
        public static void WriteLine(string s) => CorlorsPrint(s, true);

        private static void WriteAt(string s, int x, int y)
        {
            int buff_top = Console.CursorTop;
            int buff_left = Console.CursorLeft;
            Console.CursorVisible = false;
            Console.SetCursorPosition(x, y);
            //颜色代码至少占2个字符,所以长度不满3的情况下可以直接用Console输出来提升一点点效率(大概可以?)
            if (s.Length>2)
                CorlorsPrint(s);
            else
                Console.Write(s);
            Console.SetCursorPosition(buff_left, buff_top);
            Console.CursorVisible = true;
        }
        private static void WriteWhiteSpaceAt(int length, int start_x, int start_y)
        {
            if (length > 0)
            {
                StringBuilder WhiteSpace = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    WhiteSpace.Append(' ');
                }
                WriteAt(WhiteSpace.ToString(), start_x, start_y);
            }
        }
        private static void CorlorsPrint(string s, bool line = false)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '&')
                {
                    switch (s[i + 1])
                    {
                        case '0': Console.ForegroundColor = ConsoleColor.Black; i++; continue;
                        case '1': Console.ForegroundColor = ConsoleColor.DarkBlue; i++; continue;
                        case '2': Console.ForegroundColor = ConsoleColor.DarkGreen; i++; continue;
                        case '3': Console.ForegroundColor = ConsoleColor.DarkCyan; i++; continue;
                        case '4': Console.ForegroundColor = ConsoleColor.DarkRed; i++; continue;
                        case '5': Console.ForegroundColor = ConsoleColor.DarkMagenta; i++; continue;
                        case '6': Console.ForegroundColor = ConsoleColor.DarkYellow; i++; continue;
                        case '7': Console.ForegroundColor = ConsoleColor.Gray; i++; continue;
                        case '8': Console.ForegroundColor = ConsoleColor.DarkGray; i++; continue;
                        case '9': Console.ForegroundColor = ConsoleColor.Blue; i++; continue;
                        case 'a': Console.ForegroundColor = ConsoleColor.Green; i++; continue;
                        case 'b': Console.ForegroundColor = ConsoleColor.Cyan; i++; continue;
                        case 'c': Console.ForegroundColor = ConsoleColor.Red; i++; continue;
                        case 'd': Console.ForegroundColor = ConsoleColor.Magenta; i++; continue;
                        case 'e': Console.ForegroundColor = ConsoleColor.Yellow; i++; continue;
                        case 'f': Console.ForegroundColor = ConsoleColor.White; i++; continue;
                        case 'r': Console.ResetColor(); i++; continue;
                        //样式代码直接丢掉
                        case 'k': i++; continue;
                        case 'l': i++; continue;
                        case 'm': i++; continue;
                        case 'n': i++; continue;
                        case 'o': i++; continue;
                    }
                }
                Console.Write(s[i]);
                if (i >= s.Length) break;
            }
            if (line)
                Console.WriteLine();
            Console.ResetColor();
        }
        private static int GetColorCodeCount(string s)
        {
            int result = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '&')
                {
                    switch (s[i + 1])
                    {
                        //颜色代码
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
                        //样式代码
                        case 'r': result += 1; i += 2; break;
                        case 'k': result += 1; i += 2; break;
                        case 'l': result += 1; i += 2; break;
                        case 'm': result += 1; i += 2; break;
                        case 'n': result += 1; i += 2; break;
                        case 'o': result += 1; i += 2; break;
                    }
                }

            }
            return result;
        }
        private static int GetStringLength(string str)
        {
            //这和string.Length有什么区别?
            //这边计算的是输出到屏幕上占了多少长度,比如中文会占两个字这样子
            //(关于颜色代码那块,我已经看不懂我当时是怎么想的了,可是我也不敢直接重写)
            int length = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] =='&')
                {
                    switch (str[i + 1])
                    {
                        //颜色代码
                        case '0': i++; continue;
                        case '1': i++; continue;
                        case '2': i++; continue;
                        case '3': i++; continue;
                        case '4': i++; continue;
                        case '5': i++; continue;
                        case '6': i++; continue;
                        case '7': i++; continue;
                        case '8': i++; continue;
                        case '9': i++; continue;
                        case 'a': i++; continue;
                        case 'b': i++; continue;
                        case 'c': i++; continue;
                        case 'd': i++; continue;
                        case 'e': i++; continue;
                        case 'f': i++; continue;
                        //样式代码
                        case 'r': i++; continue;
                        case 'k': i++; continue;
                        case 'l': i++; continue;
                        case 'm': i++; continue;
                        case 'n': i++; continue;
                        case 'o': i++; continue;
                    }
                }
                else
                {
                    int tmp = Encoding.UTF8.GetBytes(str[i].ToString()).Length;
                    switch (tmp)
                    {
                        case 3: length += 2; break;
                        case 4: length += 2; break;
                        default: length += tmp; break;
                    }
                }
            }
            return length;
        }
        private class Line
        {
            public int y { get; set; }
            public List<Field> Fields { get; set; } = new List<Field>();
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
