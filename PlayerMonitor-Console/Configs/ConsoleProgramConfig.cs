using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayerMonitor.ConsoleOptions;
using PlayerMonitor.ConsolePlus.ColorSchemes;

namespace PlayerMonitor.Configs
{
    public abstract class ConsoleProgramConfig : Config
    {
        public virtual string WindowTitleStyle { get; protected set; }
        public virtual ConsoleColorScheme ColorScheme { get; protected set; }

        protected virtual void LoadByConsoleOptions(ReadOnlySpan<string> args)
        {
            if(args==null||args.Length==0&&this is IConsoleGuide)
            {
                IConsoleGuide Guide = this as IConsoleGuide;
                 if(!Guide.OpenGuide())
                    throw new EmptyConsoleOptionException(false);
                 
            }
        }
        protected virtual void SwitchColorScheme(ConsoleColorScheme newColorScheme)
        {
            if (!Platform.IsWindows)
            {
                throw new PlatformNotSupportedException("it need WinAPI");
            }
            else if (newColorScheme == null)
            {
                throw new ArgumentNullException(nameof(newColorScheme));
            }
            else
            {
                //后面一片白眼睛看着好难受，可能有哪里写错了。
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
    }
}
