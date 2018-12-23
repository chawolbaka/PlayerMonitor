using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using PlayersMonitor.ConsoleRewrite.ColorSchemes;

namespace PlayersMonitor
{
    //无视效率,只要不造成闪烁就可以啦
    //(QAQ我想不无视也不行呀,要是还要考虑效率的话我怕连一个可以用的版本都发不出来了)
    internal static class Screen
    {
        private static List<Line> Lines = new List<Line>();
        public static int SetDefaultForegroundColor(Color foregroundColor)
        {
            if (!SystemInfo.IsWindows)
                throw new PlatformNotSupportedException("it need WinAPI");
            else
                return WinAPI.ReplaceConsoleColor(ConsoleColor.Gray, foregroundColor.R, foregroundColor.G, foregroundColor.B);
        }
        public static int SetDefaultBackgroundColor(Color backgroundColor)
        {
            if (!SystemInfo.IsWindows)
                throw new PlatformNotSupportedException("it need WinAPI");
            else
                return WinAPI.ReplaceConsoleColor(ConsoleColor.Black, backgroundColor.R, backgroundColor.G, backgroundColor.B);
        }
        public static void SetColorScheme(ConsoleColorScheme newColorScheme)
        {
            if (!SystemInfo.IsWindows)
            {
                throw new PlatformNotSupportedException("it need WinAPI");
            }
            else if (newColorScheme == null)
            {
                throw new ArgumentNullException(nameof(newColorScheme));
            }
            else
            {
                //后面一片白眼睛看着好难受，可能有哪里学写了。
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Black, newColorScheme.Black.R, newColorScheme.Black.G, newColorScheme.Black.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Blue, newColorScheme.BrightBlue.R, newColorScheme.BrightBlue.G, newColorScheme.BrightBlue.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Cyan, newColorScheme.BrightCyan.R, newColorScheme.BrightCyan.G, newColorScheme.BrightCyan.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.DarkBlue, newColorScheme.Blue.R, newColorScheme.Blue.G, newColorScheme.Blue.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.DarkCyan, newColorScheme.Cyan.R, newColorScheme.Cyan.G, newColorScheme.Cyan.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.DarkGray, newColorScheme.BrightBlack.R, newColorScheme.BrightBlack.G, newColorScheme.BrightBlack.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.DarkGreen, newColorScheme.Green.R, newColorScheme.Green.G, newColorScheme.Green.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.DarkMagenta, newColorScheme.Magenta.R, newColorScheme.Magenta.G, newColorScheme.Magenta.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.DarkRed, newColorScheme.Red.R, newColorScheme.Red.G, newColorScheme.Red.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.DarkYellow, newColorScheme.Yellow.R, newColorScheme.Yellow.G, newColorScheme.Yellow.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Gray, newColorScheme.White.R, newColorScheme.White.G, newColorScheme.White.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Green, newColorScheme.BrightGreen.R, newColorScheme.BrightGreen.G, newColorScheme.BrightGreen.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Magenta, newColorScheme.BrightMagenta.R, newColorScheme.BrightMagenta.G, newColorScheme.BrightMagenta.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Red, newColorScheme.BrightRed.R, newColorScheme.BrightRed.G, newColorScheme.BrightRed.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.White, newColorScheme.BrightWhite.R, newColorScheme.BrightWhite.G, newColorScheme.BrightWhite.B);
                WinAPI.ReplaceConsoleColor(System.ConsoleColor.Yellow, newColorScheme.BrightYellow.R, newColorScheme.BrightYellow.G, newColorScheme.BrightYellow.B);
            }
        }

        public static string CreateLine(params string[] fields)
        {
            int y = Lines.Count > 0 ? Lines.Count : 0;
            Line NewLine = new Line();
            NewLine.y = y;
            //创建标签
            string tag_temp = Guid.NewGuid().ToString();
            while (Lines.Count > 0 && Lines.Find(l => l.Tag == tag_temp) != null)
            {
                tag_temp = Guid.NewGuid().ToString();
            }
            NewLine.Tag = tag_temp;
            //把字段都添加进去
            foreach (var fireld in fields)
            {
                NewLine.Fields.Add(new Line.Field() { Value = fireld });
            }
            //计算每个字段的3种长度
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
        public static void ReviseField(string newValue,int location, string tag)
        {
            Line FoundLine = Lines.Find(x => x.Tag == tag);
            if (FoundLine == null)
                throw new ArgumentException("Tag does not exist", nameof(tag));
            if (FoundLine.Fields[location].Value == newValue)
                return;
            int NewValueLength = GetStringLength(newValue);
            int OldValueLength = GetStringLength(FoundLine.Fields[location].Value);
            int y = FoundLine.y;
            //长度相同的话只做替换处理,如果不同的话就要重新计算长度然后把后面的全拆了QAQ
            if (NewValueLength == OldValueLength)
            {
                WriteAt(newValue, FoundLine.Fields[location].StartLocation, FoundLine.y);
            }
            else
            {
                //重新计算长度
                for (int i = location; i < FoundLine.Fields.Count; i++)
                {
                    if (i == location)
                        FoundLine.Fields[i].Length = NewValueLength;
                    else
                        FoundLine.Fields[i].Length = GetStringLength(FoundLine.Fields[i].Value);

                    FoundLine.Fields[i].StartLocation = FoundLine.Fields[i - 1].EndLocation;
                    FoundLine.Fields[i].EndLocation = FoundLine.Fields[i].StartLocation + FoundLine.Fields[i].Length;
                }
                //把新的内容输出到控制台上
                WriteAt(newValue, FoundLine.Fields[location].StartLocation, FoundLine.y);
                //重新输出一下后面的那些字段
                for (int i = location + 1; i < FoundLine.Fields.Count; i++)
                {
                    WriteAt(FoundLine.Fields[i].Value, FoundLine.Fields[i].StartLocation, y);
                }
                //清理历史残留
                int ClearLength = Console.BufferWidth - FoundLine.Fields[FoundLine.Fields.Count - 1].EndLocation;
                WriteWhiteSpaceAt(ClearLength, FoundLine.Fields[FoundLine.Fields.Count - 1].EndLocation, y);
            }
            FoundLine.Fields[location].Value = newValue;
        }
        public static void RemoveLine(string tag,bool rePirint=false)
        {
            Line FindResult = Lines.Find(x => x.Tag == tag);
            if (FindResult == null)
                throw new ArgumentOutOfRangeException(nameof(tag), $"Tag \"{tag}\" does not exist");
            Lines.Remove(FindResult);
            int ClearLength = Console.BufferWidth;

            if (Lines.Count==FindResult.y)
            {
                WriteWhiteSpaceAt(ClearLength, 0, FindResult.y);
            }
            else
            {
                for (int i = FindResult.y; i < Lines.Count; i++)
                {
                    Lines[i].y --;
                    if (rePirint)
                    {
                        WriteWhiteSpaceAt(ClearLength, 0, Lines[i].y);
                        WriteWhiteSpaceAt(ClearLength, 0, Lines[i].y + 1);
                        foreach (var Field in Lines[i].Fields)
                        {
                            WriteAt(Field.Value, Field.StartLocation, Lines[i].y);
                        }
                    }
                }
            }
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop-1);
        }
        public static bool HasLine(string tag)
        {
            Line Result = Lines.Find(x => x.Tag == tag);
            return Result == null ? false : true;
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
            bool HasColorCode = s.Contains("&");
            Console.CursorVisible = false;
            Console.SetCursorPosition(x, y);
            if (HasColorCode)
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
                    WhiteSpace.Append(" ");
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
