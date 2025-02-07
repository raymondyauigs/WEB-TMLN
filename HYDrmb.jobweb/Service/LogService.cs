
using HYDrmb.Abstraction;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Web;

namespace HYDrmb.jobweb.Service
{

    internal class ErrorLog : IErrorLog
    {
        IMemoryCache mcache;
        public ErrorLog(IMemoryCache memoryCache)
        {
            mcache = memoryCache;
        }

        public void LogError(string message, Exception ex = null)
        {
            LogService.Log(LogType.Error, message, ex, LogCategory.ErrorLog);
            //Trace.TraceError(message);
        }


    }

    internal class MiscLog : IMiscLog
    {
        IMemoryCache mcache;
        public MiscLog(IMemoryCache memoryCache)
        {
            mcache = memoryCache;
        }
        public void LogMisc(string message, Exception ex = null)
        {
            LogService.Log(LogType.Information, message, ex, LogCategory.MiscLog);
            //Trace.TraceInformation(message);
        }
    }

    internal class StdbLog : IStdbLog
    {
        public void LogStdb(string message, Exception ex = null)
        {
            LogService.Log(LogType.Information, message, ex, LogCategory.EnquiryLog);
        }
    }

    public class LogService
    {
        private static readonly ILog errorLog = LogManager.GetLogger(System.Reflection.Assembly.GetCallingAssembly(), "error");
        private static readonly ILog miscLog = LogManager.GetLogger(System.Reflection.Assembly.GetCallingAssembly(), "misc");
        private static readonly ILog stdbLog = LogManager.GetLogger(System.Reflection.Assembly.GetCallingAssembly(), "stdb");


        public static void SetupLog4net(string basepath, string configfile = "log4net.config")
        {
            File.WriteAllText($@"{basepath}\Logs\startlog.log", "Good Morning");
            var log4filepath = Path.Combine(basepath ?? AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory, configfile);
            if (File.Exists(log4filepath))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(log4filepath));
            }

        }

        public static void Log(LogType type, string message, Exception ex = null, LogCategory category = LogCategory.NotSpecify)
        {
            ILog log = GetLogger(category);
            switch (type)
            {
                case LogType.Error:
                    log.Logger.Log(typeof(LogService), log4net.Core.Level.Error, message, ex);
                    break;
                case LogType.Warning:
                    log.Logger.Log(typeof(LogService), log4net.Core.Level.Warn, message, ex);
                    break;
                case LogType.Information:
                    log.Logger.Log(typeof(LogService), log4net.Core.Level.Info, message, ex);
                    break;
                case LogType.Fatal:
                    log.Logger.Log(typeof(LogService), log4net.Core.Level.Fatal, message, ex);
                    break;
                case LogType.Debug:
                    log.Logger.Log(typeof(LogService), log4net.Core.Level.Debug, message, ex);
                    break;
            }
        }

        public static ILog GetLogger(LogCategory category = LogCategory.NotSpecify)
        {
            ILog log;
            switch (category)
            {
                case LogCategory.ErrorLog:
                    log = errorLog;
                    break;
                case LogCategory.EnquiryLog:
                    log = stdbLog;
                    break;
                case LogCategory.MiscLog:
                    log = miscLog;

                    break;

                default:
                    log = LogManager.GetLogger(new StackTrace().GetFrame(2).GetMethod().DeclaringType);
                    break;
            }
            return log;
        }

    }
}