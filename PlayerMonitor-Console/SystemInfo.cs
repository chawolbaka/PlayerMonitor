using System;
using System.Runtime.InteropServices;

namespace PlayerMonitor
{
    internal static class SystemInfo
    {
#if !DoNet
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#elif Windows
        public static bool IsWindows { get { return true; } }
#else
        public static bool IsWindows { get { return false; } }
#endif
    }
}
