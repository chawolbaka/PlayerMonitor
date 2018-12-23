using System;
using System.Drawing;

namespace PlayersMonitor.ConsoleRewrite.ColorSchemes
{
    /// <summary>
    /// 颜色名来源(2018-12-22):https://en.wikipedia.org/wiki/ANSI_escape_code#Colors
    /// </summary>
    public abstract class ConsoleColorScheme
    {

        //这些颜色上面的注释是我看着https://en.wikipedia.org/wiki/ANSI_escape_code#Colors感觉出来的。
        
        /// <summary>
        /// The Color = System.ConsoleColor.Black
        /// </summary>
        public abstract Color Black { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.DarkRed
        /// </summary>
        public abstract Color Red { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.DarkGreen
        /// </summary>
        public abstract Color Green { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.DarkYellow
        /// </summary>
        public abstract Color Yellow { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.DarkBlue
        /// </summary>
        public abstract Color Blue { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.DarkMagenta
        /// </summary>
        public abstract Color Magenta { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.DarkCyan
        /// </summary>
        public abstract Color Cyan { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.Gray
        /// </summary>
        public abstract Color White { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.DarkGray
        /// </summary>
        public abstract Color BrightBlack { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.Red
        /// </summary>
        public abstract Color BrightRed { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.Green
        /// </summary>
        public abstract Color BrightGreen { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.Yellow
        /// </summary>
        public abstract Color BrightYellow { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.Blue
        /// </summary>
        public abstract Color BrightBlue { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.Magenta
        /// </summary>
        public abstract Color BrightMagenta { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.Cyan
        /// </summary>
        public abstract Color BrightCyan { get; }
        /// <summary>
        /// The Color = System.ConsoleColor.White
        /// </summary>
        public abstract Color BrightWhite { get; }

        //MC的颜色名
        //public abstract Color Black { get; }
        //public abstract Color DarkBlue { get; }
        //public abstract Color DarkGreen { get; }
        //public abstract Color DarkAqua { get; }
        //public abstract Color DarkRed { get; }
        //public abstract Color DarkPurple { get; }
        //public abstract Color Gold { get; }
        //public abstract Color Gray { get; }
        //public abstract Color DarkGray { get; }
        //public abstract Color Blue { get; }
        //public abstract Color Green { get; }
        //public abstract Color Aqua { get; }
        //public abstract Color Red { get; }
        //public abstract Color LightPurple { get; }
        //public abstract Color Yellow { get; }
        //public abstract Color White { get; }
    }
}
