using System;
using System.Drawing;

namespace PlayerMonitor.ConsolePlus.ColorSchemes
{
    /// <summary>The Color Names Comes From:https://en.wikipedia.org/wiki/ANSI_escape_code#Colors (Date:2018-12-22)</summary>
    public abstract class ConsoleColorScheme
    {
        //这些颜色上面的注释是我看着https://en.wikipedia.org/wiki/ANSI_escape_code#Colors靠眼睛感觉出来的。
        
        /// <summary>The Color = System.ConsoleColor.Black</summary>
        public abstract Color Black { get; }
        /// <summary>The Color = System.ConsoleColor.DarkRed</summary>
        public abstract Color Red { get; }
        /// <summary>The Color = System.ConsoleColor.DarkGreen</summary>
        public abstract Color Green { get; }
        /// <summary>The Color = System.ConsoleColor.DarkYellow</summary>
        public abstract Color Yellow { get; }
        /// <summary>The Color = System.ConsoleColor.DarkBlue</summary>
        public abstract Color Blue { get; }
        /// <summary>The Color = System.ConsoleColor.DarkMagenta</summary>
        public abstract Color Magenta { get; }
        /// <summary>The Color = System.ConsoleColor.DarkCyan</summary>
        public abstract Color Cyan { get; }
        /// <summary>The Color = System.ConsoleColor.Gray</summary>
        public abstract Color White { get; }
        /// <summary>The Color = System.ConsoleColor.DarkGray</summary>
        public abstract Color BrightBlack { get; }
        /// <summary>The Color = System.ConsoleColor.Red</summary>
        public abstract Color BrightRed { get; }
        /// <summary>The Color = System.ConsoleColor.Green</summary>
        public abstract Color BrightGreen { get; }
        /// <summary>The Color = System.ConsoleColor.Yellow</summary>
        public abstract Color BrightYellow { get; }
        /// <summary>The Color = System.ConsoleColor.Blue</summary>
        public abstract Color BrightBlue { get; }
        /// <summary>The Color = System.ConsoleColor.Magenta</summary>
        public abstract Color BrightMagenta { get; }
        /// <summary>The Color = System.ConsoleColor.Cyan</summary>
        public abstract Color BrightCyan { get; }
        /// <summary>The Color = System.ConsoleColor.White</summary>
        public abstract Color BrightWhite { get; }

    }
}
