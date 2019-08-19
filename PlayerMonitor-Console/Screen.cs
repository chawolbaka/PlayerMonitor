using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using PlayerMonitor.ConsolePlus;

namespace PlayerMonitor
{
    //无视效率,只要不造成闪烁就可以啦
    //(QAQ我想不无视也不行呀,要是还要考虑效率的话我怕连一个可以用的版本都发不出来了)
    internal static class Screen
    {

        private static Dictionary<Guid, Line> Lines = new Dictionary<Guid, Line>();
        public static int Count => Lines.Count;

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
            if (fields is null || fields.Length <= 0)
                throw new ArgumentNullException(nameof(fields));
            //添加行(仅集合内)
            Line NewLine = new Line();
            NewLine.y = Lines.Count;
            Guid Tag = Guid.NewGuid();
            foreach (var fireldText in fields)
            {
                NewLine.Fields.Add(new Line.Field(fireldText));
            }
            //计算每个字段在终端中的坐标和长度
            for (int i = 0; i < NewLine.Fields.Count; i++)
            {
                NewLine.Fields[i].Length = GetStringLength(NewLine.Fields[i].Text);
                if (i == 0)
                {
                    NewLine.Fields[0].StartLocation = 0;
                }
                else if (i < NewLine.Fields.Count)
                {
                    NewLine.Fields[i].StartLocation = NewLine.Fields[i - 1].EndLocation;
                }
                else
                    break;
            }
            Lines.Add(Tag, NewLine);
            NewLine.WriteToConsole();
            Console.WriteLine();
            return Tag;
        }
        public static void ReviseField(string newField, int location, Guid tag)
        {
            Line destLine = Lines[tag];
            if (destLine.Fields[location].Text == newField)
                return;

            int NewFieldLength = GetStringLength(newField);
            int OldFieldLength = destLine.Fields[location].Length;
            int y = destLine.y;
            //长度相同的话只做替换处理,如果不同的话就要重新计算长度然后把后面的全拆了QAQ
            if (NewFieldLength == OldFieldLength)
            {
                WriteAt(newField, destLine.Fields[location].StartLocation, destLine.y);
            }
            else
            {
                //重新计算每个字段在终端中的坐标和长度
                for (int i = location; i < destLine.Fields.Count; i++)
                {
                    if (i == location)
                        destLine.Fields[i].Length = NewFieldLength;
                    else
                        destLine.Fields[i].Length = GetStringLength(destLine.Fields[i].Text);

                    destLine.Fields[i].StartLocation = destLine.Fields[i - 1].EndLocation;
                }
                //把新的字段们写入终端
                WriteAt(newField, destLine.Fields[location].StartLocation, destLine.y);
                //重新输出后面的那些字段
                for (int i = location + 1; i < destLine.Fields.Count; i++)
                {
                    WriteAt(destLine.Fields[i].Text, destLine.Fields[i].StartLocation, y);
                }
                //清理历史残留
                int ClearLength = Console.BufferWidth - destLine.Fields[destLine.Fields.Count - 1].EndLocation;
                WriteWhiteSpaceAt(ClearLength, destLine.Fields[destLine.Fields.Count - 1].EndLocation, y);
            }
            destLine.Fields[location].Text = newField;
        }
        public static void RemoveLine(Guid tag) => RemoveLine(tag, false);
        public static void RemoveLine(Guid tag, bool rePirint)
        {
            Line removeLine = Lines[tag];
            Lines.Remove(tag);
            int ClearLength = Console.BufferWidth;

            if (Lines.Count == removeLine.y)
            {
                //如果删除的是尾行就只需要清空尾行就够了
                WriteWhiteSpaceAt(ClearLength, 0, removeLine.y);
            }
            else
            {
                WriteWhiteSpaceAt(ClearLength, 0, Lines.Count);
                //这边不能直接调用Rewrite,我需要把被删除行后的所有行的y减1
                foreach (var line in Lines.Values)
                {
                    if (line.y > removeLine.y)
                    {
                        line.y--;
                        if (rePirint)
                        {
                            ClearLength = Console.BufferWidth - line.Fields[line.Fields.Count - 1].EndLocation;
                            line.WriteToConsole();
                            WriteWhiteSpaceAt(ClearLength, line.Fields[line.Fields.Count - 1].EndLocation, line.y);
                        }
                    }
                }
            }
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
        }
        public static bool HasLine(Guid tag)
        {
            return Lines.ContainsKey(tag);
        }
        /// <summary>将数据重新写入终端</summary>
        /// <param name="y">起始行(这行前面的行不会被处理)</param>
        public static void ReWrite(int y = 0)
        {
            foreach (Line line in Lines.Values)
            {
                if (line.y >= y)
                {
                    //因为我不知道当前行的字会有多长,所以直接整行清理掉了
                    WriteWhiteSpaceAt(Console.BufferWidth, 0, line.y);
                    line.WriteToConsole();
                }
            }
            Console.SetCursorPosition(0, Lines.Count <= Console.BufferHeight ? Lines.Count : Console.BufferHeight);
        }
        /// <summary>清空屏幕后重新写入终端</summary>
        public static void Refurbih()
        {
            //bug:高频率下会看见光标跑到0,0(设置CursorVisible没明显效果)
            Console.CursorVisible = false;
            ColorfullyConsole.Clear();
            ReWrite(0);
            Console.CursorVisible = true;
        }
        public static void Clear()
        {
            Lines.Clear();
            ColorfullyConsole.Clear();
        }

        

        private static void WriteAt(string str, int x, int y)
        {
            int buff_top = Console.CursorTop;
            int buff_left = Console.CursorLeft;
            Console.CursorVisible = false;
            Console.SetCursorPosition(x, y);
            //颜色代码至少占2个字符,所以长度不满3的情况下可以直接用Console.Write提升一点点效率(大概可以?)
            if (str.Length > 2)
                ColorfullyConsole.Write(str);
            else
                Console.Write(str);
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
        private static int GetStringLength(string str)
        {
            //这和string.Length有什么区别? 
            //这边给的是输出到终端后占的长度
            //大概就是颜色代码不会被计算进去,然后中文是占两个字符的。
            int length = 0;
            const byte AsciiCharLength = 1;
            const byte ChineseCharLength = 2;
            
            for (int i = 0; i < str.Length; i++)
            {
                //样式代码会直接被跳过,不会被计算进去
                if (str[i] == '&' && i != str.Length - 1)
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
                    }
                    if (ColorfullyConsole.IsFormatCodeSupport(str[i + 1]))
                    {
                        i++;
                        continue;
                    }
                }
                //如果不是样式代码的话就可以正常处理了，这边中文占2格 英文占1格
                length += Encoding.UTF8.GetByteCount(str[i].ToString()) > 1 ? ChineseCharLength : AsciiCharLength;
            }
            return length;
        }
        private class Line
        {
            public int y { get; set; }
            public List<Field> Fields { get; set; } = new List<Field>();
            public void WriteToConsole()
            {
                foreach (var field in Fields)
                {
                    WriteAt(field.Text, field.StartLocation, y);
                }
            }
            public class Field
            {
                public string Text { get; set; }
                public int StartLocation { get; set; }
                public int EndLocation => StartLocation + Length;
                /// <summary>在终端中占的长度</summary>
                public int Length { get; set; }

                public Field(string text)
                {
                    this.Text = text;
                }
                public Field(string text,int startLocation,int lenght)
                {
                    this.Text = text;
                    this.StartLocation = startLocation;
                    this.Length = lenght;
                }
                public override string ToString()
                {
                    return this.Text;
                }
                public bool Equals(Field fidld)
                {
                    if (fidld.Length != this.Length)
                        return false;
                    else if (fidld.StartLocation != this.StartLocation)
                        return false;
                    else
                        return fidld.Text.Equals(this.Text);
                }
            }
        }
    }
}
