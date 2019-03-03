using System.Drawing;

namespace PlayerMonitor.ConsolePlus.ColorSchemes
{
    public class CMDColorScheme : ConsoleColorScheme
    {
        public override Color Black => Color.FromArgb(0, 0, 0);
        public override Color Red => Color.FromArgb(128, 0, 0);
        public override Color Green => Color.FromArgb(0, 128, 0);
        public override Color Yellow => Color.FromArgb(128, 128, 0);
        public override Color Blue => Color.FromArgb(0, 0, 128);
        public override Color Magenta => Color.FromArgb(128, 0, 128);
        public override Color Cyan => Color.FromArgb(0, 128, 128);
        public override Color White => Color.FromArgb(192, 192, 192);
        public override Color BrightBlack => Color.FromArgb(128, 128, 128);
        public override Color BrightRed => Color.FromArgb(255, 0, 0);
        public override Color BrightGreen => Color.FromArgb(0, 255, 0);
        public override Color BrightYellow => Color.FromArgb(255, 255, 0);
        public override Color BrightBlue => Color.FromArgb(0, 0, 255);
        public override Color BrightMagenta => Color.FromArgb(255, 0, 255);
        public override Color BrightCyan => Color.FromArgb(0, 255, 255);
        public override Color BrightWhite => Color.FromArgb(255, 255, 255);
    }
}