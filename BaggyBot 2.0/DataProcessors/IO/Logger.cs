using System;
using System.Text;
using System.IO;
using BaggyBot.Tools;

namespace BaggyBot
{
	public enum LogLevel
	{
		Debug,
		Info,
		Message,
		Irc,
		Warning,
		Error
	}

	public delegate void LogEvent(string message, LogLevel level);

	public static class Logger
	{
        public static bool UseColouredOutput { get; set; }
		public const string LogFileName = "baggybot.log";
		private static bool disposed;
		private static string prefix = string.Empty;
		private const int prefixLength = 52;

		public static event LogEvent OnLogEvent;

		static Logger()
		{
			LoadLogFile();
		}
		private static void LoadLogFile()
		{
			textWriter = new StreamWriter(LogFileName, true);
		}

		private static TextWriter textWriter;

		public static void SetPrefix(string prefix)
		{
			Logger.prefix = prefix;
		}

		public static void ClearPrefix()
		{
			prefix = string.Empty;
		}

        private const string KNRM = "\x1B[0m";
        private const string KRED = "\x1B[31m";
        private const string KGRN = "\x1B[32m";
        private const string KYEL = "\x1B[33m";
        private const string KBLU = "\x1B[34m";
        private const string KMAG = "\x1B[35m";
        private const string KCYN = "\x1B[36m";
        private const string KWHT = "\x1B[37m";
        private const string RESET = "\x33[0m";

		public static void Log(object sender, string message, LogLevel level = LogLevel.Debug, bool writeLine = true, params object[] format)
		{
			if (format.Length != 0) {
				message = String.Format(message, format);
			}
			var lineBuilder = new StringBuilder();
			//var lineColor = ConsoleColor.Gray;
		    var lineColor = "";

			switch (level) {
				case LogLevel.Debug:
					lineBuilder.Append("[DEB] ");
			        lineColor = KWHT;
					break;
				case LogLevel.Info:
					lineBuilder.Append("[INF] ");
			        lineColor = KGRN;
					break;
				case LogLevel.Message:
					lineBuilder.Append("[MSG] ");
			        lineColor = KBLU;
					break;
				case LogLevel.Irc:
					lineBuilder.Append("[IRC] ");
			        lineColor = KNRM;
					break;
				case LogLevel.Warning:
					lineBuilder.Append("[WRN] ");
			        lineColor = KYEL;
					break;
				case LogLevel.Error:
					lineBuilder.Append("[ERR] ");
			        lineColor = KRED;
					break;
			}
			lineBuilder.Append(prefix);

			
			if (sender != null)
			{
				var time = DateTime.Now.ToString("[MMM dd - HH:mm:ss.fff] ");
				var location = string.Format("[{0}-{1:X4}] ", sender.GetType().Name.Truncate(16), sender.GetHashCode());
				lineBuilder.Insert(0, (time + location).PadRight(prefixLength));
			}
			else
			{
				lineBuilder.Insert(0, "".PadRight(prefixLength));
			}
			lineBuilder.Append(message);

			WriteToConsole(lineColor, level, lineBuilder);

			if ((level == LogLevel.Error || level == LogLevel.Warning) && OnLogEvent != null) {
				OnLogEvent(lineBuilder.ToString(), level);
			}
		    WriteToLogFile(lineBuilder, writeLine);
		}

		private static void WriteToLogFile(StringBuilder lineBuilder, bool writeLine)
		{
			if (!disposed) {
				if(writeLine)
					textWriter.WriteLine(lineBuilder.ToString());
				else
					textWriter.Write(lineBuilder.ToString());
				textWriter.Flush();
			}
		}

		private static void WriteToConsole(string lineColor, LogLevel level, StringBuilder lineBuilder)
		//private static void WriteToConsole(ConsoleColor lineColor, LogLevel level, StringBuilder lineBuilder)
		{
			//var prevColor = Console.ForegroundColor;
			//Console.ForegroundColor = lineColor;
            if (UseColouredOutput)
            {
                Console.Write(lineColor);
            }

			if (level == LogLevel.Debug) {
				var writeDebug = false;
				if (bool.TryParse(Settings.Instance["show_debug_log"], out writeDebug)) {
					if (writeDebug) {
						Console.WriteLine(lineBuilder.ToString());
					}
				} else {
					Log(null, "Unable to parse settings value for show_debug_log into a boolean.", LogLevel.Warning);
					Console.WriteLine(lineBuilder.ToString());
				}
			} else {
				Console.WriteLine(lineBuilder.ToString());
			}
            //Console.Write(RESET);
			//Console.ForegroundColor = prevColor;
		}

		public static void ClearLog()
		{
			textWriter.Close();
			File.Delete(LogFileName);
			LoadLogFile();
		}
		public static void Dispose()
		{
			Log(null, "Shutting down logger", LogLevel.Info);
			textWriter.Close();
			textWriter.Dispose();
			disposed = true;
		}
    }
}
