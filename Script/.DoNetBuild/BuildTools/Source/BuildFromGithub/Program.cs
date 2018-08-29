using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BuildFromGithub
{
    class Program
    {
        static void Main(string[] args)
        {
            //string XmlFile = args[0];
            //string Source = args[1];
            //string Destination = args[2];

            string Source_Github = args[0];
            string Source_Build = args[1];
            string Destination = args[2];
            string XmlFile = args[3];

            //string Source_Github = @"C:\Users\Chawol\Desktop\.DoNetBuild\tmp\MinecraftProtocol\MinecraftProtocol";
            //string Source_Build = @"C:\Users\Chawol\Desktop\.DoNetBuild\MinecraftProtocol";
            //string Destination = @"C:\Users\Chawol\Desktop\.DoNetBuild\tmp\Build";
            //string XmlFile = Source_Build+ "\\MinecraftProtocol.csproj";
            if (Directory.Exists(Destination))
            Directory.Delete(Destination, true);
            CopyDirectory(Source_Build, Destination);

            XmlDocument xml = new XmlDocument();
            xml.Load(XmlFile);
            XmlNode root = xml.DocumentElement;
            XmlElement ItemGroup = xml.CreateElement("ItemGroup");
            foreach (var fileInfo in GetFiles(Source_Github))
            {
                string NewFilePath = fileInfo.FullName.ToString().Replace(Source_Github, Destination);
                //移植文件
                if (!Directory.Exists(fileInfo.DirectoryName.ToString().Replace(Source_Github, Destination)))
                    Directory.CreateDirectory(fileInfo.DirectoryName.ToString().Replace(Source_Github, Destination));
                if (File.Exists(NewFilePath))
                    File.Delete(NewFilePath);
                File.Copy(fileInfo.FullName, NewFilePath);
                
                //写入xml
                XmlElement cs = xml.CreateElement("Compile");
                cs.SetAttribute("Include", fileInfo.FullName.ToString().Replace($"{Source_Github}\\", ""));
                ItemGroup.AppendChild(cs);
            }
            root.AppendChild(ItemGroup);
            using (MemoryStream stream = new MemoryStream())
            {
                xml.Save(stream);
                using (StreamWriter sw = new StreamWriter(
                    XmlFile.Replace(Source_Build, Destination),false, new UTF8Encoding(false)))
                {
                    sw.Write(Encoding.UTF8.GetString(stream.ToArray()).
                        Replace(" xmlns=\"\"", ""));
                }
            }
        }
        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            //https://www.cnblogs.com/leco/archive/2010/11/18/1881315.html
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            Directory.CreateDirectory(destinationPath);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = Path.Combine(destinationPath, fsi.Name);

                if (fsi is FileInfo)
                    File.Copy(fsi.FullName, destName);
                else
                {
                    Directory.CreateDirectory(destName);
                    CopyDirectory(fsi.FullName, destName);
                }
            }
        }
        static List<FileInfo> GetFiles(string path)
        {
            List<FileInfo> result = new List<FileInfo>();
            foreach (var file in  Directory.GetFiles(path))
            {
                if (Regex.Match(file,@"\.cs$").Success)
                {
                    result.Add(new FileInfo(file));
                }
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (!dir.Contains("obj")&& !dir.Contains("bin")&&!dir.Contains("Properties"))
                {
                    result.AddRange(GetFiles(dir));
                }
            }
            return result;
        }
    }
}
