using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
namespace BaggyBot
{
	public delegate void LogEvent(string message, LogLevel level);

	public static class Logger
	{
		private static string filename = "baggybot.log";
		private static bool disposed = false;

		public static event LogEvent OnLogEvent;

		static Logger()
		{
			LoadLogFile();
		}
		private static void LoadLogFile()
		{
			textWriter = new StreamWriter(filename, true);
		}

		private static TextWriter textWriter;

		public static void Log(string message, LogLevel level = LogLevel.Debug, bool writeLine = true)
		{
			StringBuilder lineBuilder = new StringBuilder();
			ConsoleColor lineColor = ConsoleColor.Gray;

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
			lineBuilder.Append(message);

			WriteToConsole(lineColor, level, writeLine, lineBuilder);

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

		private static void WriteToConsole(ConsoleColor lineColor, LogLevel level, bool writeLine, StringBuilder lineBuilder)
		{
			bool writeDebug = false;
			bool.TryParse(Settings.Instance["show_debug_log"], out writeDebug);
			var prevColor = Console.ForegroundColor;
			Console.ForegroundColor = lineColor;
			if (level != LogLevel.Debug || writeDebug) {
				if (writeLine)
					Console.WriteLine(lineBuilder.ToString());
				else
					Console.Write(lineBuilder.ToString());
			}
			Console.ForegroundColor = prevColor;
		}

		public static void ClearLog()
		{
			textWriter.Close();
			File.Delete(filename);
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
