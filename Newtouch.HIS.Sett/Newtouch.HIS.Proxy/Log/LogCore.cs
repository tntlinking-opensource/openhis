using System;
using System.Collections.Generic;
using Newtouch.Core.Common.Utils;
using NLog.Client;
using NLog.Contact.DTO;
using System.IO;
using System.Configuration;

namespace Newtouch.HIS.Proxy.Log
{
    /// <summary>
    /// 日志组件
    /// </summary>
    public class LogCore
    {
        private static readonly ILog Logger = LogProxy.GetLogger(string.IsNullOrWhiteSpace(ConfigurationHelper.GetAppConfigValue("ServiceName")) ? "Newtouch.Sett" : ConfigurationHelper.GetAppConfigValue("ServiceName"));

        private static string xnhLogAddress =
            string.IsNullOrEmpty(ConfigurationManager.AppSettings["xnhLogAddress"])
                ? "C:\\HISLog\\log_xinnonghe_guigan"
                : ConfigurationManager.AppSettings["xnhLogAddress"];
        private static string LogAddress =
            string.IsNullOrEmpty(ConfigurationManager.AppSettings["settLogAddress"])
                ? "C:\\HISLog\\log_sett_web"
                : ConfigurationManager.AppSettings["settLogAddress"];
        public static void Info<T>(string title, T message, Dictionary<string, string> addInfo = null)
        {
            Logger.Info(title, message, tags: addInfo);
        }

        internal static void Debug(string title, string message = "", Dictionary<string, string> addInfo = null)
        {
            Logger.Debug(title, message, tags: addInfo);
        }

        public static void Error(string title, Exception ex = null, string message = "", Dictionary<string, string> addInfo = null)
        {
            if (ex != null && string.IsNullOrWhiteSpace(message))
            {
                message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }
            Logger.Error(title, message, ex, addInfo);
        }

        public static void Fatal(string title, Exception ex = null, string message = "", Dictionary<string, string> addInfo = null)
        {
            if (ex != null && string.IsNullOrWhiteSpace(message))
            {
                message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }
            Logger.Fatal(title, message, ex, addInfo);
        }

        public static void Warn(string title, Exception ex = null, string message = "", Dictionary<string, string> addInfo = null)
        {
            Logger.Warn(title, message, ex, addInfo);
        }

        public static void Moniter(string title, object message, MoniterDTO moniter, Dictionary<string, string> addInfo = null)
        {
            Logger.Moniter(title, message, moniter, addInfo);
        }

        /// <summary>
        /// 写本地日志
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfo(string message)
        {
            try
            {
                var date = DateTime.Now.ToString("yyyyMMddHHmm");
                var dirPath = string.Format("{0}\\{1}", xnhLogAddress, date.Substring(0, 8));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                var filePath = string.Format("{0}\\{1}.txt", dirPath, date.Substring(8, 3));
                File.AppendAllText(filePath, string.Format("\r\n\r\n{0}.Info.{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
            }
            catch { }
        }
        /// <summary>
        /// 写本地日志
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfoWeb(string message)
        {
            try
            {
                var date = DateTime.Now.ToString("yyyyMMddHHmm");
                var dirPath = string.Format("{0}\\{1}", LogAddress, date.Substring(0, 8));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                var filePath = string.Format("{0}\\{1}.txt", dirPath, date.Substring(8, 3));
                File.AppendAllText(filePath, string.Format("\r\n\r\n{0}.Info.{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
            }
            catch { }
        }
    }
}