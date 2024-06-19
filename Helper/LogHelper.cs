using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace skill_composer.Helper
{
    /// <summary>
    /// Provides utility methods for logging messages to various log files.
    /// </summary>
    public static class LogHelper
    {
        private static readonly BlockingCollection<LogItem> LogQueue = new BlockingCollection<LogItem>();
        private static readonly string LogFilePath = Path.Combine(FilePathHelper.GetRootDirectory(), "logCombined.txt");
        private static Task loggingTask;
        private static CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initialises the log helper class.
        /// </summary>
        static LogHelper()
        {
            cancellationTokenSource = new CancellationTokenSource();
            loggingTask = Task.Run(() => ProcessLogQueue(cancellationTokenSource.Token));
        }

        /// <summary>
        /// Logs a message with the specified log type.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="logType">The type of log.</param>
        public static void Log(string message, LogType logType = LogType.General)
        {
            var serviceTypeLogPath = Path.Combine(FilePathHelper.GetRootDirectory(), $"log{logType}.txt");

            if (logType != LogType.Debug && logType != LogType.TwilioWebsocket && logType != LogType.AssemblyAIWebsocket)
            {
                Console.WriteLine($"{logType} {message}");
            }

            message = message.TrimEnd() + Environment.NewLine; // Ensure message ends with a newline

            LogQueue.Add(new LogItem { FilePath = serviceTypeLogPath, Message = message });

            if (logType != LogType.Debug && logType != LogType.TwilioWebsocket && logType != LogType.AssemblyAIWebsocket)
            {
                // combined logging
                LogQueue.Add(new LogItem { FilePath = LogFilePath, Message = message });
            }
        }

        /// <summary>
        /// Logs an error message with the specified log type.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="logType">The type of log.</param>
        public static void Error(string message, LogType logType = LogType.General)
        {
            message = "ERROR " + message;
            Log(message, logType);
        }

        /// <summary>
        /// Stops the logging process.
        /// </summary>
        public static void StopLogging()
        {
            cancellationTokenSource.Cancel();
            LogQueue.CompleteAdding();
            loggingTask.Wait();
        }

        /// <summary>
        /// Gets the total execution time since the specified start time.
        /// </summary>
        /// <param name="start">The start time of execution.</param>
        /// <returns>A tuple containing the total execution time and the end date and time of execution.</returns>
        public static (TimeSpan totalTime, DateTime executionEndDateTime) GetTotalExecutionTime(DateTime start)
        {
            var end = DateTime.Now;
            var ts = (end - start);
            return (ts, end);
        }

        private static void ProcessLogQueue(CancellationToken cancellationToken)
        {
            foreach (var logItem in LogQueue.GetConsumingEnumerable(cancellationToken))
            {
                TryAppendTextWithRetry(logItem.FilePath, logItem.Message);
            }
        }

        private static void TryAppendTextWithRetry(string filePath, string message, int retryInterval = 10,
            int maxAttempts = 10)
        {
            for (int attempts = 0; attempts < maxAttempts; attempts++)
            {
                try
                {
                    File.AppendAllText(filePath, message);
                    break; // Success, exit the loop
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // ERROR_SHARING_VIOLATION
                {
                    if (attempts < maxAttempts - 1)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    else
                    {
                        Console.WriteLine($"Unable to write to log file after {maxAttempts} attempts: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred while writing to the log file: {ex.Message}");
                    break;
                }
            }
        }

        private class LogItem
        {
            public string FilePath { get; set; }
            public string Message { get; set; }
        }
    }

    /// <summary>
    /// Represents the type of log.
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Database related log.
        /// </summary>
        Database,

        /// <summary>
        /// HTTP related log.
        /// </summary>
        Http,

        Calls,

        /// <summary>
        /// General log.
        /// </summary>
        General,

        /// <summary>
        /// AIFunction
        /// </summary>
        AIFunction,

        /// <summary>
        /// TwilioWebsocket
        /// </summary>
        TwilioWebsocket,

        /// <summary>
        /// AssemblyAIWebsocket
        /// </summary>
        AssemblyAIWebsocket,


        /// <summary>
        /// Debug log (does not write to the console, but does log to the logdebug.txt file).
        /// </summary>
        Debug,

        /// <summary>
        /// Log related to latency.
        /// </summary>
        Latency
    }
}