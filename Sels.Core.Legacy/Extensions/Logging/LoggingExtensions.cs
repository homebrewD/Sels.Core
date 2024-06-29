using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sels.Core.Logging;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Sels.Core.Extensions.Text;
using Sels.Core.Models.Disposables;
using Sels.Core.Extensions.Collections;

namespace Sels.Core.Extensions.Logging
{
    /// <summary>
    /// Exposes some simple additonal logging methods on ILoggers.
    /// </summary>
    public static class LoggingExtensions
    {
        #region Logger
        #region Message
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogMessage(this ILogger logger, LogLevel level, string message, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, message, args);
                }
            }
            catch { }
        }
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogMessage(this ILogger logger, LogLevel level, Func<string> messageFunc, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, messageFunc(), args);
                }
            }
            catch { }
        }
        #endregion

        #region Exception
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="exception">The exception to log</param>
        public static void LogException(this ILogger logger, LogLevel level, Exception exception)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, null);
                }
            }
            catch { }
        }
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogException(this ILogger logger, LogLevel level, string message, Exception exception, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, message, args);
                }
            }
            catch { }
        }
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogException(this ILogger logger, LogLevel level, Func<string> messageFunc, Exception exception, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, messageFunc(), args);
                }
            }
            catch { }
        }
        #endregion

        #region Object
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this ILogger logger, LogLevel level, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level) & objects.HasValue())
                {
                    logger.Log(level, objects.Where(x => x.HasValue()).Select(x => JsonConvert.SerializeObject(x)).JoinStringNewLine());
                }
            }
            catch { }
        }
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this ILogger logger, LogLevel level, string message, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, Helper.Strings.JoinStringsNewLine(message, objects.Where(x => x.HasValue()).Select(x => JsonConvert.SerializeObject(x)).JoinStringNewLine()));
                }
            }
            catch { }
        }
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this ILogger logger, LogLevel level, string message, Exception exception, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, Helper.Strings.JoinStringsNewLine(message, objects.Where(x => x.HasValue()).Select(x => JsonConvert.SerializeObject(x)).JoinStringNewLine()));
                }
            }
            catch { }
        }
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this ILogger logger, LogLevel level, Func<string> messageFunc, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, Helper.Strings.JoinStringsNewLine(messageFunc(), objects.Where(x => x.HasValue()).Select(x => JsonConvert.SerializeObject(x)).JoinStringNewLine()));
                }
            }
            catch { }
        }
        /// <summary>
        /// Logs using <paramref name="logger"/> if it is not null and enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this ILogger logger, LogLevel level, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, Helper.Strings.JoinStringsNewLine(messageFunc(), objects.Where(x => x.HasValue()).Select(x => JsonConvert.SerializeObject(x)).JoinStringNewLine()));
                }
            }
            catch { }
        }
        #endregion
        #endregion

        #region Loggers
        #region Message
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogMessage(this IEnumerable<ILogger> loggers, LogLevel level, string message, params object[] args)
        {
            loggers.ForceExecute(x => x.LogMessage(level, message, args));
        }
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogMessage(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, params object[] args)
        {
            loggers.ForceExecute(x => x.LogMessage(level, messageFunc, args));
        }
        #endregion

        #region Exception
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="exception">The exception to log</param>
        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, Exception exception)
        {
            loggers.ForceExecute(x => x.LogException(level, exception));
        }
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, string message, Exception exception, params object[] args)
        {
            loggers.ForceExecute(x => x.LogException(level, message, exception, args));
        }
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="args">Optional arguments for formatting the log message</param>
        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, Exception exception, params object[] args)
        {
            loggers.ForceExecute(x => x.LogException(level, messageFunc, exception, args));
        }
        #endregion

        #region Object
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, objects));
        }
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, string message, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, message, objects));
        }
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, string message, Exception exception, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, message, exception, objects));
        }
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, messageFunc, objects));
        }
        /// <summary>
        /// Logs when <paramref name="loggers"/> is not null and with any logger enabled for log level <paramref name="level"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <param name="level">The log level for the message</param>
        /// <param name="messageFunc">The delegate that returns the message to log</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="objects">The objects to serialize and log</param>
        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, messageFunc, exception, objects));
        }
        #endregion
        #endregion

        #region Timed Logger
        /// <summary>
        /// Creates a <see cref="TimedLogger"/> with <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The logger to create the <see cref="TimedLogger"/> with</param>
        /// <param name="logLevel">The log level for the logs</param>
        /// <param name="beginMessage">The message to log when the <see cref="TimedLogger"/> gets created</param>
        /// <param name="endMessageFunc">The delegate that returns the message to log when the returned <see cref="TimedLogger"/> is stopped or disposed</param>
        /// <returns>The <see cref="TimedLogger"/> created from <paramref name="logger"/></returns>
        public static TimedLogger CreateTimedLogger(this ILogger logger, LogLevel logLevel, string beginMessage, Func<TimeSpan, string> endMessageFunc)
        {
            if (logger == null || !logger.IsEnabled(logLevel)) return new NullTimedLogger();
            return new StopWatchTimedLogger(logger, logLevel, () => beginMessage, endMessageFunc);
        }
        /// <summary>
        /// Creates a <see cref="TimedLogger"/> with <paramref name="loggers"/>.
        /// </summary>
        /// <param name="loggers">The loggers to create the <see cref="TimedLogger"/> with</param>
        /// <param name="logLevel">The log level for the logs</param>
        /// <param name="beginMessage">The message to log when the <see cref="TimedLogger"/> gets created</param>
        /// <param name="endMessageFunc">The delegate that returns the message to log when the returned <see cref="TimedLogger"/> is stopped or disposed</param>
        /// <returns>The <see cref="TimedLogger"/> created from <paramref name="loggers"/></returns>
        public static TimedLogger CreateTimedLogger(this IEnumerable<ILogger> loggers, LogLevel logLevel, string beginMessage, Func<TimeSpan, string> endMessageFunc)
        {
            if (loggers == null || !loggers.Any(x => x.IsEnabled(logLevel))) return new NullTimedLogger();
            return new StopWatchTimedLogger(loggers, logLevel, () => beginMessage, endMessageFunc);
        }
        /// <summary>
        /// Creates a <see cref="TimedLogger"/> with <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The logger to create the <see cref="TimedLogger"/> with</param>
        /// <param name="logLevel">The log level for the logs</param>
        /// <param name="beginMessageFunc">The delegate that returns the message to log when the <see cref="TimedLogger"/> gets created</param>
        /// <param name="endMessageFunc">The delegate that returns the message to log when the returned <see cref="TimedLogger"/> is stopped or disposed</param>
        /// <returns>The <see cref="TimedLogger"/> created from <paramref name="logger"/></returns>
        public static TimedLogger CreateTimedLogger(this ILogger logger, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
        {
            if (logger == null || !logger.IsEnabled(logLevel)) return new NullTimedLogger();
            return new StopWatchTimedLogger(logger, logLevel, beginMessageFunc, endMessageFunc);
        }
        /// <summary>
        /// Creates a <see cref="TimedLogger"/> with <paramref name="loggers"/>.
        /// </summary>
        /// <param name="loggers">The loggers to create the <see cref="TimedLogger"/> with</param>
        /// <param name="logLevel">The log level for the logs</param>
        /// <param name="beginMessageFunc">The delegate that returns the message to log when the <see cref="TimedLogger"/> gets created</param>
        /// <param name="endMessageFunc">The delegate that returns the message to log when the returned <see cref="TimedLogger"/> is stopped or disposed</param>
        /// <returns>The <see cref="TimedLogger"/> created from <paramref name="loggers"/></returns>
        public static TimedLogger CreateTimedLogger(this IEnumerable<ILogger> loggers, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
        {
            if (loggers == null || !loggers.Any(x => x.IsEnabled(logLevel))) return new NullTimedLogger();
            return new StopWatchTimedLogger(loggers, logLevel, beginMessageFunc, endMessageFunc);
        }
        #endregion

        #region Scope
        /// <summary>
        /// Tries to begin a logging scope with <paramref name="scope"/> if <paramref name="logger"/> is not null.
        /// </summary>
        /// <param name="logger">The logger to start the scope with</param>
        /// <param name="scope">Dictionary containing the logging parameters</param>
        /// <returns>A logging scope started with <paramref name="scope"/> or an instance of <see cref="NullDisposer"/> if <paramref name="logger"/> is null</returns>
        public static IDisposable TryBeginScope(this ILogger logger, Dictionary<string, object> scope)
        {
            scope.ValidateArgument(nameof(scope));

            if (logger == null) return NullDisposer.Instance;

            return logger.BeginScope(scope);
        }
        /// <summary>
        /// Tries to begin a logging scope with <paramref name="scope"/> if <paramref name="logger"/> is not null.
        /// </summary>
        /// <param name="logger">The logger to start the scope with</param>
        /// <param name="scope">Dictionary containing the logging parameters</param>
        /// <returns>A logging scope started with <paramref name="scope"/> or an instance of <see cref="NullDisposer"/> if <paramref name="logger"/> is null</returns>
        public static IDisposable TryBeginScope(this ILogger logger, IDictionary<string, object> scope)
        {
            scope.ValidateArgument(nameof(scope));

            if (logger == null) return NullDisposer.Instance;

            return logger.BeginScope(scope);
        }
        /// <summary>
        /// Tries to begin a logging scope with <paramref name="scope"/> if <paramref name="logger"/> is not null.
        /// </summary>
        /// <param name="logger">The logger to start the scope with</param>
        /// <param name="scope">Dictionary containing the logging parameters</param>
        /// <returns>A logging scope started with <paramref name="scope"/> or an instance of <see cref="NullDisposer"/> if <paramref name="logger"/> is null</returns>
        public static IDisposable TryBeginScope(this ILogger logger, IReadOnlyDictionary<string, object> scope)
        {
            scope.ValidateArgument(nameof(scope));

            if (logger == null) return NullDisposer.Instance;

            return logger.BeginScope(scope);
        }

        /// <summary>
        /// Tries to begin a logging scope with <paramref name="scope"/> if <paramref name="logger"/> is not null.
        /// </summary>
        /// <param name="logger">The logger to start the scope with</param>
        /// <param name="scope">Key/value pairs containing the logging parameters</param>
        /// <returns>A logging scope started with <paramref name="scope"/> or an instance of <see cref="NullDisposer"/> if <paramref name="logger"/> is null</returns>
        public static IDisposable TryBeginScope(this ILogger logger, params KeyValuePair<string, object>[] scope)
        {
            scope.ValidateArgument(nameof(scope));

            if (logger == null) return NullDisposer.Instance;

            var parameters = new Dictionary<string, object>();
            scope.Execute(x => parameters.AddOrUpdate(x.Key, x.Value));

            return logger.BeginScope(scope);
        }
        /// <summary>
        /// Tries to begin a logging scope with the scope configured by <paramref name="scopeBuilder"/> if <paramref name="logger"/> is not null.
        /// </summary>
        /// <param name="logger">The logger to start the scope with</param>
        /// <param name="scopeBuilder">Delegate that is used to configure the logging scope</param>
        /// <returns>A logging scope started with the scope configured by <paramref name="scopeBuilder"/> or an instance of <see cref="NullDisposer"/> if <paramref name="logger"/> is null</returns>
        public static IDisposable TryBeginScope(this ILogger logger, Action<IDictionary<string,object>> scopeBuilder)
        {
            if (logger == null) return NullDisposer.Instance;
            scopeBuilder.ValidateArgument(nameof(scopeBuilder));
            var scope = new Dictionary<string, object>();
            scopeBuilder(scope);

            return logger.TryBeginScope(scope);
        }

        /// <summary>
        /// Tries to begin a logging scope with <paramref name="key"/> and <paramref name="value"/> if <paramref name="logger"/> is not null.
        /// </summary>
        /// <param name="logger">The logger to start the scope with</param>
        /// <param name="key">The name of the log parameter</param>
        /// <param name="value">The value of the log parameter</param>
        /// <returns>A logging scope started with a single key/value pair created from <paramref name="key"/> and <paramref name="value"/> or an instance of <see cref="NullDisposer"/> if <paramref name="logger"/> is null</returns>
        public static IDisposable TryBeginScope(this ILogger logger, string key, object value)
        {
            if (logger == null) return NullDisposer.Instance;
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return logger.TryBeginScope(x => x.Add(key, value));
        }
        #endregion
    }
}
