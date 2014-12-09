using System;
using System.Text;
using System.IO;
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
		public const string LogFileName = "baggybot.log";
		private static bool disposed;
		private static string prefix = string.Empty;

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

		public static void Log(string message, LogLevel level = LogLevel.Debug, bool writeLine = true, params object[] format)
		{
			if (format.Length != 0) {
				message = String.Format(message, format);
			}
			var lineBuilder = new StringBuilder();
			var lineColor = ConsoleColor.Gray;

			switch (level) {
				case LogLevel.Debug:
					lineBuilder.Append("[DEB]\t");
					lineColor = ConsoleColor.Gray;
					break;
				case LogLevel.Info:
					lineBuilder.Append("[INF]\t");
					lineColor = ConsoleColor.Green;
					break;
				case LogLevel.Message:
					lineBuilder.Append("[MSG]\t");
					lineColor = ConsoleColor.DarkGreen;
					break;
				case LogLevel.Irc:
					lineBuilder.Append("[IRC]\t");
					lineColor = ConsoleColor.White;
					break;
				case LogLevel.Warning:
					lineBuilder.Append("[WRN]\t");
					lineColor = ConsoleColor.Yellow;
					break;
				case LogLevel.Error:
					lineBuilder.Append("[ERR]\t");
					lineColor = ConsoleColor.DarkRed;
					break;
			}
			lineBuilder.Append(prefix);
			lineBuilder.Append(message);

			WriteToConsole(lineColor, level, lineBuilder);

			if ((level == LogLevel.Error || level == LogLevel.Warning) && OnLogEvent != null) {
				OnLogEvent(lineBuilder.ToString(), level);
			}
			lineBuilder.Insert(0, DateTime.Now.ToString("[MMM dd - HH:mm:ss.fff]\t"));
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

		private static void WriteToConsole(ConsoleColor lineColor, LogLevel level, StringBuilder lineBuilder)
		{
			var prevColor = Console.ForegroundColor;
			Console.ForegroundColor = lineColor;
			if (level == LogLevel.Debug) {
				var writeDebug = false;
				if (bool.TryParse(Settings.Instance["show_debug_log"], out writeDebug)) {
					if (writeDebug) {
						Console.WriteLine(lineBuilder.ToString());
					}
				} else {
					Log("Unable to parse settings value for show_debug_log into a boolean.", LogLevel.Warning);
					Console.WriteLine(lineBuilder.ToString());
				}
			} else {
				Console.WriteLine(lineBuilder.ToString());
			}
			Console.ForegroundColor = prevColor;
		}

		public static void ClearLog()
		{
			textWriter.Close();
			File.Delete(LogFileName);
			LoadLogFile();
		}
		public static void Dispose()
		{
			Log("Shutting down logger", LogLevel.Info);
			textWriter.Close();
			textWriter.Dispose();
			disposed = true;
		}
	}
}
