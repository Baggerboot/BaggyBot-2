using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
namespace BaggyBot
{
	static class Logger
	{
		private static string filename = "baggybot.log";
		private static bool disposed = false;

		static Logger()
		{
			LoadLogFile();
		}
		private static void LoadLogFile()
		{
			textWriter = new StreamWriter(filename, true);
		}

		private static TextWriter textWriter;

		internal static void Log(string message, LogLevel level = LogLevel.Debug)
		{
			StringBuilder lineBuilder = new StringBuilder();
			ConsoleColor lineColor = ConsoleColor.Gray;

			switch (level) {
				case LogLevel.Debug:
					lineBuilder.Append("[DEB]\t");
					lineColor = ConsoleColor.White;
					break;
				case LogLevel.Info:
					lineBuilder.Append("[INF]\t");
					lineColor = ConsoleColor.Green;
					break;
				case LogLevel.Warning:
					lineBuilder.Append("[WRN]\t");
					lineColor = ConsoleColor.Yellow;
					break;
				case LogLevel.Error:
					lineBuilder.Append("[ERR]\t");
					lineColor = ConsoleColor.Red;
					break;
			}
			lineBuilder.Append(message);

			bool writeDebug = false;
			bool.TryParse(Settings.Instance["show_debug_log"], out writeDebug);
			if (!Program.noColor) {
				Console.ForegroundColor = lineColor;
			}
			if (level != LogLevel.Debug || writeDebug) {
				Console.WriteLine(lineBuilder.ToString());
			}

			if (!disposed) {
				textWriter.WriteLine(lineBuilder.ToString());
				textWriter.Flush();
			}
		}
		internal static void ClearLog()
		{
			textWriter.Close();
			File.Delete(filename);
			LoadLogFile();
		}
		internal static void Dispose()
		{
			textWriter.Close();
			textWriter.Dispose();
			disposed = true;
		}
	}
}
