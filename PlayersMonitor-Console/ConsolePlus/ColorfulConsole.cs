using System;
using System.Collections.Generic;
using System.Text;

namespace PlayersMonitor.ConsolePlus
{
    public static class ColorfulConsole
    {
        //以后这边要重写彩色输出的部分,现在先引用一下旧的.
        public static void Write(string s) => Screen.Write(s);
        public static void WriteLine(string s) => Screen.WriteLine(s);
    }
}
