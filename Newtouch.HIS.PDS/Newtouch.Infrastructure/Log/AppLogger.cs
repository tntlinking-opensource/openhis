using System;
using System.IO;

namespace Newtouch.Infrastructure.Log
{
    /// <summary>
    /// 日志组件
    /// </summary>
    public class AppLogger
    {
        public static void Info(string message)
        {
            try
            {
                var root = "C:\\HISLog\\log_yibao\\PDS";
                var date = DateTime.Now.ToString("yyyyMMddHHmm");
                var dirPath = string.Format("{0}\\{1}", root, date.Substring(0, 8));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                var filePath = string.Format("{0}\\{1}.txt", dirPath, date.Substring(8, 2));
                File.AppendAllText(filePath, string.Format("\r\n\r\n{0}.Info.{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
            }
            catch { }
        }
    }
}