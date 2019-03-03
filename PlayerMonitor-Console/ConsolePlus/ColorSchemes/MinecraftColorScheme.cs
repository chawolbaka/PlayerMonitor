using System;
using System.Drawing;

namespace PlayerMonitor.ConsolePlus.ColorSchemes
{
    public class MinecraftColorScheme:ConsoleColorScheme
    {
        public override Color Black => _black;

        public override Color Red => _darkRed;

        public override Color Green => _darkGreen;

        public override Color Yellow => _gold;

        public override Color Blue => _darkBlue;

        public override Color Magenta => _darkPurple;

        public override Color Cyan => _darkAqua;

        public override Color White => _gray;

        public override Color BrightBlack => _darkGray;

        public override Color BrightRed => _red;

        public override Color BrightGreen => _green;

        public override Color BrightYellow => _yellow;

        public override Color BrightBlue => _blue;

        public override Color BrightMagenta => _lightPurple;

        public override Color BrightCyan => _aqua;

        public override Color BrightWhite => _white;

        private readonly Color _black = Color.FromArgb(0, 0, 0);
        private readonly Color _darkBlue = Color.FromArgb(0, 0, 170);
        private readonly Color _darkGreen = Color.FromArgb(0, 170, 0);
        private readonly Color _darkAqua = Color.FromArgb(0, 170, 170);
        private readonly Color _darkRed = Color.FromArgb(170, 0, 0);
        private readonly Color _darkPurple = Color.FromArgb(170, 0, 170);
        private readonly Color _gold = Color.FromArgb(255, 170, 0);
        private readonly Color _gray = Color.FromArgb(170, 170, 170);
        private readonly Color _darkGray = Color.FromArgb(85, 85, 85);
        private readonly Color _blue = Color.FromArgb(85, 85, 255);
        private readonly Color _green = Color.FromArgb(85, 255, 85);
        private readonly Color _aqua = Color.FromArgb(85, 255, 255);
        private readonly Color _red = Color.FromArgb(255, 85, 85);
        private readonly Color _lightPurple = Color.FromArgb(255, 85, 255);
        private readonly Color _yellow = Color.FromArgb(255, 255, 85);
        private readonly Color _white = Color.FromArgb(255, 255, 255);
    }
}
