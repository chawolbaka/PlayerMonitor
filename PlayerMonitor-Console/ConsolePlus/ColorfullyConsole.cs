using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PlayerMonitor.ConsolePlus
{
    public static class ColorfullyConsole
    {
        private static IntPtr OutputHandle;
        private static bool IsWindows10;
        //这两行就是为什么要在程序开启的时候需要执行一下Init的原因,如果在执行Init前设置了Console中的这两项属性会导致ResetColor无法还原默认色
        private static ConsoleColor DefaultForegroundColor = Console.ForegroundColor;
        private static ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;

        public static bool UseCompatibilityMode { get; set; }
        public const char DefaultColorCodeMark = '&';

        //static ColorfullyConsole() => Init();

        public static void Init()
        {
#if COMPATIBILITY_MODE
            UseCompatibilityMode = true;
#else

            if (Platform.IsWindows)
            {
            #if !CORE_RT
                IsWindows10 = IsWindows10 = Environment.OSVersion.Version.Major >= 10;
            #endif
                if (IsWindows10)
                {
                    WinAPI.GetConsoleMode(OutputHandle, out uint consoleMode);
                    WinAPI.SetConsoleMode(OutputHandle, consoleMode | (uint)WinAPI.ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
                }
                else
                {
                    OutputHandle = WinAPI.GetStdHandle(WinAPI.STD_OUTPUT_HANDLE);
                }
            }
#endif
        }

        //样式代码
        public static void Write(string value, char colorCodeMark)
        {
            //这些处理样式代码的方法效率是不怎么好的,所以我觉得能不用的话尽量不去用吧(比如只有空格的情况就直接用Console.Write输出了)
            if (string.IsNullOrEmpty(value))
                return;
            else if (string.IsNullOrWhiteSpace(value))
                Console.Write(value);
#if COMPATIBILITY_MODE
            else
                WriteColorCode(value, colorCodeMark);
#else
            //兼容模式也有可能是通过命令行选项开启的,所以需要保留这个else if
            else if (UseCompatibilityMode)
                WriteColorCode(value, colorCodeMark);
            else if (IsWindows10)
                WriteColorCodeInWindows10(value, colorCodeMark);
            else if (Platform.IsWindows)
                WriteColorCodeInWindows(value, colorCodeMark);
            else if (Platform.IsLinux)
                WriteColorCodeInLinux(value, colorCodeMark);
            else
                WriteColorCode(value, colorCodeMark);//其它系统交给.net core去兼容.
#endif

        }
        public static void Write(object value, char colorCodeMark) => Write(value.ToString(), colorCodeMark);
        public static void Write(string value) => Write(value, DefaultColorCodeMark);
        public static void Write(object value) => Write(value.ToString(), DefaultColorCodeMark);
  

        //???(不知道怎么描述)
        public static void Write(string value, ConsoleColor fgColor, bool resetColor)
        {
            if (resetColor)
            {
                ConsoleColor buff_fg = Console.ForegroundColor;
                if (Console.ForegroundColor != fgColor)
                {
                    buff_fg = Console.ForegroundColor;
                    Console.ForegroundColor = fgColor;
                }
                Console.Write(value);
                if (Console.ForegroundColor != buff_fg)
                    Console.ForegroundColor = buff_fg;
            }
            else
            {
                if (Console.ForegroundColor != fgColor)
                    Console.ForegroundColor = fgColor;
                Console.Write(value);
            }
        }
        public static void Write(string value, ConsoleColor fgColor, ConsoleColor bgColor, bool resetColor)
        {
            if (resetColor)
            {
                ConsoleColor buff_fg = Console.ForegroundColor;
                ConsoleColor buff_bg = Console.BackgroundColor;
                if (Console.ForegroundColor != fgColor)
                {
                    buff_fg = Console.ForegroundColor;
                    Console.ForegroundColor = fgColor;
                }
                if (Console.BackgroundColor != bgColor)
                {
                    buff_bg = Console.BackgroundColor;
                    Console.BackgroundColor = bgColor;
                }
                Console.Write(value);
                if (Console.ForegroundColor != buff_fg)
                    Console.ForegroundColor = buff_fg;
                if (Console.BackgroundColor != buff_bg)
                    Console.BackgroundColor = buff_bg;
            }
            else
            {
                if (Console.ForegroundColor != fgColor)
                    Console.ForegroundColor = fgColor;
                if (Console.BackgroundColor != bgColor)
                    Console.BackgroundColor = bgColor;
                Console.Write(value);
            }
        }
        public static void Write(string value, ConsoleColor fgColor) => Write(value, fgColor, true);
        public static void Write(object value, ConsoleColor fgColor) => Write(value.ToString(), fgColor, true);
        public static void Write(object value, ConsoleColor fgColor, bool resetColor) => Write(value.ToString(), fgColor, resetColor);
        public static void Write(string value, ConsoleColor fgColor, ConsoleColor bgColor) => Write(value, fgColor, bgColor, true);
        public static void Write(object value, ConsoleColor fgColor, ConsoleColor bgColor) => Write(value.ToString(), fgColor, bgColor, true);
        public static void Write(object value, ConsoleColor fgColor, ConsoleColor bgColor, bool resetColor) => Write(value.ToString(), fgColor, bgColor, resetColor);

        //RGB
        public static void Write(string value, Color fgColor) => WriteRGBInForeground(value,fgColor.R, fgColor.G, fgColor.B,true);
        public static void Write(object value, Color fgColor) => WriteRGBInForeground(value.ToString(), fgColor.R, fgColor.G, fgColor.B, true);
        public static void Write(string value, Color fgColor, bool resetColor) => WriteRGBInForeground(value, fgColor.R, fgColor.G, fgColor.B,resetColor);
        public static void Write(object value, Color fgColor, bool resetColor) => WriteRGBInForeground(value.ToString(), fgColor.R, fgColor.G, fgColor.B, resetColor);
        public static void Write(string vlaue, Color fgColor, Color bgColor) => WriteRGB(vlaue, fgColor.R, fgColor.G, fgColor.B, bgColor.R, bgColor.G, bgColor.B,true);
        public static void Write(object vlaue, Color fgColor, Color bgColor) => WriteRGB(vlaue.ToString(), fgColor.R, fgColor.G, fgColor.B, bgColor.R, bgColor.G, bgColor.B, true);
        public static void Write(string value, Color fgColor, Color bgColor, bool resetColor) => WriteRGB(value, fgColor.R, fgColor.G, fgColor.B, bgColor.R, bgColor.G, bgColor.B, resetColor);
        public static void Write(object value, Color fgColor, Color bgColor, bool resetColor) => WriteRGB(value.ToString(), fgColor.R, fgColor.G, fgColor.B, bgColor.R, bgColor.G, bgColor.B, resetColor);


        
        public static void WriteLine(string value, char colorCodeMark)
        {
            //value的结尾会加上"\033[0m"我想把换行写在这句后面,而不是前面(虽然视觉上好像没什么区别
            Write(value, colorCodeMark);
            Console.Write(Environment.NewLine);
        }
        public static void WriteLine(string value, ConsoleColor fgColor, bool resetColor)
        {
            Write(value + Environment.NewLine, fgColor, resetColor);
        }
        public static void WriteLine(string value, ConsoleColor fgColor, ConsoleColor bgColor, bool resetColor)
        {
            Write(value + Environment.NewLine, fgColor, bgColor, resetColor);
        }
        public static void WriteLine(string value, Color fgColor, bool resetColor)
        {
            Write(value + Environment.NewLine, fgColor, resetColor);
        }
        public static void WriteLine(string value, Color fgColor, Color bgColor, bool resetColor)
        {
            Write(value + Environment.NewLine, fgColor, bgColor, resetColor);
        }
        //一堆为了让我写舒服点的重载(看着眼瞎)
        public static void WriteLine(string value) => WriteLine(value, DefaultColorCodeMark);
        public static void WriteLine(object value) => WriteLine(value.ToString(),DefaultColorCodeMark);
        public static void WriteLine(object value, char colorCodeMark) => WriteLine(value.ToString(), colorCodeMark);        
        public static void WriteLine(string value, ConsoleColor fgColor) => WriteLine(value, fgColor, true);
        public static void WriteLine(object value, ConsoleColor fgColor) => WriteLine(value.ToString(), fgColor, true);
        public static void WriteLine(object value, ConsoleColor fgColor, bool resetColor) => WriteLine(value.ToString(), fgColor, resetColor);
        public static void WriteLine(string value, ConsoleColor fgColor, ConsoleColor bgColor) => WriteLine(value, fgColor, bgColor, true);
        public static void WriteLine(object value, ConsoleColor fgColor, ConsoleColor bgColor) => WriteLine(value.ToString(), fgColor, bgColor, true);
        public static void WriteLine(object value, ConsoleColor fgColor, ConsoleColor bgColor, bool resetColor) => WriteLine(value.ToString(), fgColor, bgColor, resetColor);       
        public static void WriteLine(string value, Color fgColor) => WriteLine(value, fgColor, true);
        public static void WriteLine(object value, Color fgColor) => WriteLine(value.ToString(), fgColor, true);
        public static void WriteLine(object value, Color fgColor, bool resetColor) => WriteLine(value.ToString(), fgColor, resetColor);
        public static void WriteLine(string value, Color fgColor, Color bgColor) => WriteLine(value, fgColor, bgColor, true);
        public static void WriteLine(object value, Color fgColor, Color bgColor) => WriteLine(value.ToString(), fgColor, bgColor, true);
        public static void WriteLine(object value, Color fgColor, Color bgColor, bool resetColor) => WriteLine(value.ToString(), fgColor, bgColor, resetColor);

        public static void ResetColor() => ResetColor(DefaultForegroundColor, DefaultBackgroundColor);
        public static void ResetColor(ConsoleColor defForegroundColor, ConsoleColor defBackgroundColor)
        {
#if COMPATIBILITY_MODE
                Console.ResetColor();
#else
            if (UseCompatibilityMode)
                Console.ResetColor();
            else if (Platform.IsLinux || IsWindows10)
                Console.Write(ANSI.EscapeCode.ColorOff);
            else if (Platform.IsWindows) //如果设置了下划线之类的属性会导致Console.ResetColor()只能去除下划线无法恢复默认颜色
                WinAPI.SetConsoleTextAttribute(OutputHandle, (ushort)((ushort)defForegroundColor | (ushort)defBackgroundColor << 4));
            else
                Console.ResetColor();
#endif
        }
        public static void Clear()
        {
            if (!UseCompatibilityMode && Platform.IsLinux )
            {
                //bug:CoreRT编译的虽然有清屏的效果,但是\x1b[2J会被显示出来
                Console.Write(ANSI.EscapeCode.CleanScreen);
                //我写完\x1b[2J后再写同样长度的东西回去是不是就可以解决这个问题?
                //不过这样子光标在高频率下感觉会来回闪一下
                Console.SetCursorPosition(0, 0);
            }
            else
            {
                Console.Clear();
            }
        }

        /// <summary>获取当前平台下可用的样式代码数</summary>
        public static (int ColorCodeCount,int FormatCodeCount) GetCodeCount(string value,char colorCodeMark)
        {
            int ColorCodeCount = 0;
            int FormatCodeCount = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i]==colorCodeMark&& i!=value.Length-1)
                {
                    if (!UseCompatibilityMode&&Platform.IsWindows)
                    {
                        switch (value[i + 1])
                        {
                            case '0': ColorCodeCount++; i++; break;
                            case '1': ColorCodeCount++; i++; break;
                            case '2': ColorCodeCount++; i++; break;
                            case '3': ColorCodeCount++; i++; break;
                            case '4': ColorCodeCount++; i++; break;
                            case '5': ColorCodeCount++; i++; break;
                            case '6': ColorCodeCount++; i++; break;
                            case '7': ColorCodeCount++; i++; break;
                            case '8': ColorCodeCount++; i++; break;
                            case '9': ColorCodeCount++; i++; break;
                            case 'a': ColorCodeCount++; i++; break;
                            case 'b': ColorCodeCount++; i++; break;
                            case 'c': ColorCodeCount++; i++; break;
                            case 'd': ColorCodeCount++; i++; break;
                            case 'e': ColorCodeCount++; i++; break;
                            case 'f': ColorCodeCount++; i++; break;
                            case 'n': FormatCodeCount++; i++; break;
                            case 'r': FormatCodeCount++; i++; break;
                        }
                    }
                    else if(!UseCompatibilityMode&&Platform.IsLinux)
                    {
                        switch (value[i + 1])
                        {
                            case '0': ColorCodeCount++; i++; break;
                            case '1': ColorCodeCount++; i++; break;
                            case '2': ColorCodeCount++; i++; break;
                            case '3': ColorCodeCount++; i++; break;
                            case '4': ColorCodeCount++; i++; break;
                            case '5': ColorCodeCount++; i++; break;
                            case '6': ColorCodeCount++; i++; break;
                            case '7': ColorCodeCount++; i++; break;
                            case '8': ColorCodeCount++; i++; break;
                            case '9': ColorCodeCount++; i++; break;
                            case 'a': ColorCodeCount++; i++; break;
                            case 'b': ColorCodeCount++; i++; break;
                            case 'c': ColorCodeCount++; i++; break;
                            case 'd': ColorCodeCount++; i++; break;
                            case 'e': ColorCodeCount++; i++; break;
                            case 'f': ColorCodeCount++; i++; break;
                            case 'l': FormatCodeCount++; i++; break;
                            case 'o': FormatCodeCount++; i++; break;
                            case 'n': FormatCodeCount++; i++; break;
                            case 'm': FormatCodeCount++; i++; break;
                            case 's': FormatCodeCount++; i++; break;
                            case 't': FormatCodeCount++; i++; break;
                            case 'r': FormatCodeCount++; i++; break;
                        }
                    }
                    else
                    {
                        switch (value[i + 1])
                        {
                            case '0': ColorCodeCount++; i++; break;
                            case '1': ColorCodeCount++; i++; break;
                            case '2': ColorCodeCount++; i++; break;
                            case '3': ColorCodeCount++; i++; break;
                            case '4': ColorCodeCount++; i++; break;
                            case '5': ColorCodeCount++; i++; break;
                            case '6': ColorCodeCount++; i++; break;
                            case '7': ColorCodeCount++; i++; break;
                            case '8': ColorCodeCount++; i++; break;
                            case '9': ColorCodeCount++; i++; break;
                            case 'a': ColorCodeCount++; i++; break;
                            case 'b': ColorCodeCount++; i++; break;
                            case 'c': ColorCodeCount++; i++; break;
                            case 'd': ColorCodeCount++; i++; break;
                            case 'e': ColorCodeCount++; i++; break;
                            case 'f': ColorCodeCount++; i++; break;
                            case 'r': FormatCodeCount++; i++; break;
                        }
                    }
                }
            }
            return (ColorCodeCount, FormatCodeCount);
        }

        /// <summary>获取当前平台下可用的样式代码数</summary>
        public static (int ColorCodeCount, int FormatCodeCount) GetCodeCount(string value) => GetCodeCount(value, DefaultColorCodeMark);

        /// <summary>查询样式代码在当前平台下是否被支持</summary>
        /// <param name="formatCode">带标识符的样式代码</param>
        public static bool IsFormatCodeSupport(string formatCode)
        {
            if (formatCode.Length == 2 && formatCode[0] == DefaultColorCodeMark)
                return IsFormatCodeSupport(formatCode[1]);
            else if (formatCode.Length == 1 && formatCode[0] != DefaultColorCodeMark)
                return IsFormatCodeSupport(formatCode[0]);
            else if (formatCode.Length > 2)
                throw new ArgumentOutOfRangeException(nameof(formatCode), 2, "FormatCode too big");
            else
                return false;
        }

        /// <summary>查询样式代码在当前平台下是否被支持</summary>
        /// <param name="formatCode">无标识符的样式代码</param>
        public static bool IsFormatCodeSupport(char formatCode)
        {
            if (formatCode == 'r')
                return true;
            else if (UseCompatibilityMode)
                return false;
            else if (Platform.IsWindows)
            {
                return formatCode == 'n';
            }
            else if (Platform.IsLinux)
            {
                switch (formatCode)
                {
                    case 'l': return true;
                    case 'o': return true;
                    case 'n': return true;
                    case 'm': return true;
                    case 's': return true;
                    case 't': return true;
                }
            }
            return false;
        }

        //使用SetConsoleTextAttribute实现
        private static void WriteColorCodeInWindows(string s, char mark)
        {
            //思路:
            //缓存需要输出的文本>遇到颜色代码>输出缓存的内容并清空缓存>设置颜色(会叠加,如果有多个颜色代码叠在一起)>回到第一步
            //由于设计的是遇到颜色代码才输出缓存内的东西,所以如果后面一直没颜色代码就会遍历结束的时候输出
            //为什么不直接输出而要缓存?
            //因为我不想一个个字出现,想尽量一次性输出那些字
            StringBuilder sb = new StringBuilder();
            ushort DefaultColorAttribute = (ushort)((ushort)DefaultForegroundColor | (ushort)DefaultBackgroundColor << 4);
            ushort ColorAttribute = DefaultColorAttribute;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == mark && i != s.Length - 1)
                {
                    Console.Write(sb);
                    sb.Clear();
                    //在设置颜色前需要把颜色位清理,不清0的话会遇到这种问题:15|1=15(下划线只能通过&r取消)
                    if (s[i + 1] != 'n')
                        ColorAttribute &= WinAPI.ConsoleColorAttributes.COMMON_LVB_UNDERSCORE | 0b1111_0000;
                    switch (s[i + 1])
                    {
                        case '0': ColorAttribute |= 0x0; i++; break; //Black
                        case '1': ColorAttribute |= 0x1; i++; break; //DarkBlue
                        case '2': ColorAttribute |= 0x2; i++; break; //DarkGreen
                        case '3': ColorAttribute |= 0x3; i++; break; //DarkCyan
                        case '4': ColorAttribute |= 0x4; i++; break; //DarkRed
                        case '5': ColorAttribute |= 0x5; i++; break; //DarkMagenta
                        case '6': ColorAttribute |= 0x6; i++; break; //DarkYellow
                        case '7': ColorAttribute |= 0x7; i++; break; //Gray
                        case '8': ColorAttribute |= 0x8; i++; break; //DarkGray
                        case '9': ColorAttribute |= 0x9; i++; break; //Blue
                        case 'a': ColorAttribute |= 0xA; i++; break; //Green
                        case 'b': ColorAttribute |= 0xB; i++; break; //Cyan
                        case 'c': ColorAttribute |= 0xC; i++; break; //Red
                        case 'd': ColorAttribute |= 0xD; i++; break; //Magenta
                        case 'e': ColorAttribute |= 0xE; i++; break; //Yellow
                        case 'f': ColorAttribute |= 0xF; i++; break; //White
                        case 'n': ColorAttribute |= WinAPI.ConsoleColorAttributes.COMMON_LVB_UNDERSCORE; i++; break;
                        case 'r': ColorAttribute = DefaultColorAttribute; ResetColor(); i++; continue;
                        default: Console.Write(mark); continue;
                    }
                    WinAPI.SetConsoleTextAttribute(OutputHandle, ColorAttribute);
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            //防止有人写"&x"这种东西(x=0~f/r/n)
            if (sb.Length > 0)
                Console.Write(sb);
            ResetColor();
        }
        //使用ANSI实现
        private static void WriteColorCodeInLinux(string s, char mark)
        {
            //linux可以一次性输出,不需要和win一样先设置属性再输出字,所以简单了一点(结果还是被我写的很烂)
            StringBuilder OutputText = new StringBuilder();
            int TextColor = -1;
            List<int> TextModes = new List<int>();

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i]==mark&&i!=s.Length-1)
                {
                    //如果mark后是有效的颜色代码就会在设置好后直接跳出循环,如果不是就写入OutputText
                    switch (s[i+1])
                    {
                        //8Color
                        case '0': TextColor = 30; i++; continue; //Black
                        case '1': TextColor = 34; i++; continue; //DarkBlue
                        case '2': TextColor = 32; i++; continue; //DarkGreen
                        case '3': TextColor = 36; i++; continue; //DarkCyan
                        case '4': TextColor = 31; i++; continue; //DarkRed
                        case '5': TextColor = 35; i++; continue; //DarkMagenta
                        case '6': TextColor = 33; i++; continue; //DarkYellow
                        case '8': TextColor = 37; i++; continue; //DarkGray
                        //16Color
                        case '7': TextColor = 90; i++; continue; //Gray
                        case '9': TextColor = 94; i++; continue; //Blue
                        case 'a': TextColor = 92; i++; continue; //Green
                        case 'b': TextColor = 96; i++; continue; //Cyan
                        case 'c': TextColor = 91; i++; continue; //Red
                        case 'd': TextColor = 95; i++; continue; //Magenta
                        case 'e': TextColor = 93; i++; continue; //Yellow
                        case 'f': TextColor = 97; i++; continue; //White
                        //FormatCode
                        case 'l': TextModes.Add(1); i++; continue; //bold
                        case 'o': TextModes.Add(3); i++; continue; //italic
                        case 'n': TextModes.Add(4); i++; continue; //underline
                        case 's': TextModes.Add(5); i++; continue; //blinking (slow)
                        case 't': TextModes.Add(6); i++; continue; //blinking (fast)
                        case 'm': TextModes.Add(9); i++; continue; //cross-out
                        case 'r': TextColor = 0; TextModes.Clear(); i++; continue; //0=reset
                    }                    
                }
                

                //如果有不止一个样式代码的情况就需要在开头把它们叠一下
                if (TextModes.Count > 1)
                {
                    foreach (int mode in TextModes)
                    {
                        OutputText.Append($"\x1b[{mode}m");
                    }
                }
                //有颜色代码并且样式代码只有一种
                if (TextColor >= 0 && TextModes.Count == 1)
                {
                    OutputText.Append($"\x1b[{TextModes[0]};{TextColor}m");
                }
                //只有样式代码
                else if (TextColor < 0 && TextModes.Count == 1)
                {
                    OutputText.Append($"\x1b[{TextModes[0]}m");
                }
                //只有颜色代码
                //样式代码不止一种并且有颜色代码
                else if (TextColor >= 0)
                {
                    OutputText.Append($"\x1b[{TextColor}m");
                }

                OutputText.Append(s[i]);

                //恢复默认状态
                TextColor = -1;
                TextModes.Clear();
            }

            if (OutputText.Length > 0)
                OutputText.Append(ANSI.EscapeCode.ColorOff);//重置颜色和样式
            Console.Write(OutputText);
        }
        //使用ANSI实现
        private static void WriteColorCodeInWindows10(string s, char mark)
        {
            //Windows10不知道哪个版本开始好像是支持ANSI了
            //这种直接拼接好一次性写到终端在视觉上比先设置颜色在写入这样子的循环感觉稍微好一点
            StringBuilder OutputText = new StringBuilder();
            int TextColor = -1;
            int TextMode = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == mark && i != s.Length - 1)
                {
                    //如果mark后是有效的颜色代码就会在设置好后直接跳出循环,如果不是就写入OutputText
                    switch (s[i + 1])
                    {
                        //8Color
                        case '0': TextColor = 30; i++; continue; //Black
                        case '1': TextColor = 34; i++; continue; //DarkBlue
                        case '2': TextColor = 32; i++; continue; //DarkGreen
                        case '3': TextColor = 36; i++; continue; //DarkCyanh
                        case '4': TextColor = 31; i++; continue; //DarkRed
                        case '5': TextColor = 35; i++; continue; //DarkMagenta
                        case '6': TextColor = 33; i++; continue; //DarkYellow
                        case '8': TextColor = 37; i++; continue; //DarkGray
                        //16Color
                        case '7': TextColor = 90; i++; continue; //Gray
                        case '9': TextColor = 94; i++; continue; //Blue
                        case 'a': TextColor = 92; i++; continue; //Green
                        case 'b': TextColor = 96; i++; continue; //Cyan
                        case 'c': TextColor = 91; i++; continue; //Red
                        case 'd': TextColor = 95; i++; continue; //Magenta
                        case 'e': TextColor = 93; i++; continue; //Yellow
                        case 'f': TextColor = 97; i++; continue; //White
                        //FormatCode
                        case 'n': TextMode = 4; i++; continue; //underline
                        case 'r': TextColor = -1; TextMode = 0; OutputText.Append(ANSI.EscapeCode.ColorOff); i++; continue; //0=reset
                    }
                }

                if (TextColor>0&&TextMode>0)
                {
                    OutputText.Append($"\x1b[{TextMode};{TextColor}m");
                    TextColor = -1;
                    TextMode = 0;
                }
                else if(TextColor>0&&TextMode<=0)
                {
                    OutputText.Append($"\x1b[{TextColor}m");
                    TextColor = -1;
                }
                else if(TextColor<0&&TextMode>0)
                {
                    OutputText.Append($"\x1b[{TextMode}m");
                    TextMode = 0;
                }
                OutputText.Append(s[i]);
            }

            if (OutputText.Length > 0)
                OutputText.Append(ANSI.EscapeCode.ColorOff);//重置颜色和样式
            Console.Write(OutputText);
        }
        //使用Console类实现
        private static void WriteColorCode(string s, char mark)
        {
            StringBuilder Output = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if(s[i]==mark&&i!=s.Length-1)
                {
                    Console.Write(Output);
                    Output.Clear();
                    switch (s[i+1])
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
                    }
                }
                Output.Append(s[i]);
            }
            if (Output.Length > 0)
                Console.Write(Output);
            Console.ResetColor();
        }

        private static void WriteRGB(string s, byte fgRed, byte fgGreen, byte fgBlue, byte bgRed, byte bgGreen, byte bgBlue, bool resetColor)
        {
            if (!UseCompatibilityMode&&(Platform.IsLinux||IsWindows10))
            {
                Console.Write(ANSI.GetForegroundColorCode(fgRed, fgGreen, fgBlue));
                Console.Write(ANSI.GetBackgroundColorCode(bgRed, bgGreen, bgBlue));
                Console.Write(s);
                if (resetColor)
                    Console.Write(ANSI.EscapeCode.ColorOff);
            }
            else
            {
                //高版本的Win10是好像支持ANSI的,但是我暂时懒的查了。
                Console.ForegroundColor = GetClosestConsoleColor(fgRed, fgGreen, fgBlue);
                Console.BackgroundColor = GetClosestConsoleColor(bgRed, bgGreen, bgBlue);
                Console.Write(s);
                if (resetColor)
                    Console.ResetColor();
            }
        }
        private static void WriteRGBInForeground(string s, byte r, byte g, byte b, bool resetColor)
        {
            if (!UseCompatibilityMode&&(Platform.IsLinux||IsWindows10))
            {
                if (!resetColor)
                    Console.Write(ANSI.GetForegroundColorCode(r, g, b) + s);
                else
                    Console.Write(ANSI.GetForegroundColorCode(r, g, b) + s + ANSI.EscapeCode.ColorOff);
            }
            else
            {
                Console.ForegroundColor = GetClosestConsoleColor(r, g, b);
                Console.Write(s);
                if (resetColor)
                    Console.ResetColor();
            }
        }
        private static void WriteRGBInBackground(string s, byte r, byte g, byte b, bool resetColor)
        {
            if (!UseCompatibilityMode&&(Platform.IsLinux||IsWindows10))
            {
                if (!resetColor)
                    Console.Write(ANSI.GetBackgroundColorCode(r, g, b) + s);
                else
                    Console.Write(ANSI.GetBackgroundColorCode(r, g, b) + s + ANSI.EscapeCode.ColorOff);
            }
            else
            {
                Console.ForegroundColor = GetClosestConsoleColor(r, g, b);
                Console.Write(s);
                if (resetColor)
                    Console.ResetColor();
            }
        }
        private static ConsoleColor GetClosestConsoleColor(byte r, byte g, byte b)
        {
            //by:Glenn Slayden (https://stackoverflow.com/questions/1988833/converting-color-to-consolecolor)
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }
}
