using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerMonitor.ConsolePlus
{
    public static class ANSI
    {
        //https://stackoverflow.com/questions/5947742/how-to-change-the-output-color-of-echo-in-linux

        public static string GetForegroundColorCode(byte r, byte g, byte b)
        {
            return $"\x1b[38;2;{r};{g};{b}m";
        }
        public static string GetBackgroundColorCode(byte r, byte g, byte b)
        {
            return $"\x1b[48;2;{r};{g};{b}m";
        }
        public static class EscapeCode
        {
            public const string ColorOff = "\x1b[0m";
            public const string CleanScreen = "\x1b[2J";
        }
    }
}