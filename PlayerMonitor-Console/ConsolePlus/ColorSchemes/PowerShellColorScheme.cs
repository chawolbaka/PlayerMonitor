using System;
using System.Drawing;

namespace PlayerMonitor.ConsolePlus.ColorSchemes
{
    public class PowerShellColorScheme : CMDColorScheme
    {
        //根据(https://en.wikipedia.org/wiki/ANSI_escape_code#Colors)上写的RGB来看,PowerShell只有2种颜色的RGB和CMD的不一样,所以我就直接从CMD那边派生了
        //_(:з」∠)_PowerShell是CMD的进化版，这样子说好像也没什么问题吧（好像有点问题,不过不知道怎么准确描述。
        public override Color Yellow => Color.FromArgb(238, 237, 240);
        public override Color Magenta => Color.FromArgb(1, 36, 86);
    }
}