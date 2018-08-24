using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PlayersMonitor
{

    internal static class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public static extern int MessageBox(int hWnd, string strMsg, string strCaption, int iType);

    }
}
