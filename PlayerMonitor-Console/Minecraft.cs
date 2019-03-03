using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PlayerMonitor
{
    public static class Minecraft
    {
        public const ushort DefaultPortOfServer = 25565;

        //问题:会匹配到x-.cn这种不符合规范的域名(应该是不符合的,我查到的内容是出现-的情况头尾必须是(a-zOR0-9)这种的)
        private static readonly string REG_Domain = @"^(?=^.{3,255}$)[a-zA-Z0-9\u0800-\u4e00\u4E00-\u9FA5][\-a-zA-Z0-9\u0800-\u4e00\u4E00-\u9FA5]{0,62}(\.[a-zA-Z0-9\u0800-\u4e00\u4E00-\u9FA5]{1,62})+\.?$";
        private static readonly string REG_DomainTopLevelOnly = @"^([a-zA-Z0-9\u0800-\u4e00\u4E00-\u9FA5]{1,62})\.$"; //只有顶级域名的情况(为了防止变成什么都被匹配,需要写上根域的.)
        private static readonly string REG_IPv4 = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";
        private static readonly string REG_HasPort = @".+(：|:)([1-9]\d{0,3}|[1-5]\d{0,4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])\s*$";

        /// <summary>
        /// 简单模拟MC的地址解析(不负责解析域名,仅用于分离服务器地址和端口)
        /// </summary>
        /// <param name="serverAddress">用户输入的地址</param>
        /// <returns>如果无法解析会返回null</returns>
        public static (string Host,ushort Port)? ServerAddressResolve(string serverAddress)
        {
            //用户输入为纯IPv4或域名
            if (Regex.Match(serverAddress, REG_IPv4).Success|| Regex.Match(serverAddress, REG_Domain).Success||Regex.Match(serverAddress, REG_DomainTopLevelOnly).Success)
            {
                return (serverAddress, DefaultPortOfServer);
            }
            else if (Regex.Match(serverAddress, REG_HasPort).Success) //用户输入携带端口号的服务器地址
            {
                string Host = serverAddress.Contains(":")? serverAddress.Split(":")[0] : serverAddress.Split("：")[0];
                ushort Port = ushort.Parse(Regex.Replace(serverAddress, REG_HasPort, "$2"));
                //验证“:”前面是不是IP或者域名.
                if (Regex.Match(Host, REG_IPv4).Success || Regex.Match(Host, REG_Domain).Success)
                    return (Host, Port);
                else
                    return null;//这边的理想的情况是用户输入了：端口号这种情况,这种情况下IP我总不能塞个127.0.0.1吧...
            }
            else return null;//用户脸滚键盘
        }
    }
}