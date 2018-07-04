using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ES.LoggingTools
{
    public class NLogLogger : ILog
    {
        private readonly Logger _log;

        public NLogLogger(Type type)
        {
            _log = LogManager.GetLogger(type.FullName);
        }

        public void Debug(string format, params object[] args)
        {
            Log(LogLevel.Debug, format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log(LogLevel.Info, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log(LogLevel.Warn, format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log(LogLevel.Error, format, args);
        }

        public void Error(Exception ex)
        {
            Log(LogLevel.Error, null, null, ex);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Error, format, args, ex);
        }

        public void Fatal(string format, params object[] args)
        {
            Log(LogLevel.Fatal, format, args);
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Fatal, format, args, ex);
        }

        private void Log(LogLevel level, string format, object[] args)
        {
            _log.Log(typeof(NLogLogger), new LogEventInfo(level, _log.Name, null, format, args));
        }

        private void Log(LogLevel level, string format, object[] args, Exception ex)
        {
            _log.Log(typeof(NLogLogger), new LogEventInfo(level,
                                                           _log.Name,
                                                           null,
                                                           "{0} \r\n{1}",
                                                           new object[] { string.Format(format, args),
                                                                          ex.StackTrace }
                                                           )
                     );
            //_log.Log(typeof(NLogLogger), new LogEventInfo(level, _log.Name, null, "{0}", new object[] { ex.StackTrace }));
        }
    }
}