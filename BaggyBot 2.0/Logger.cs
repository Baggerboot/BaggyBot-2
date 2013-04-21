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
			string line = "";
			ConsoleColor lineColor = ConsoleColor.Gray;

			switch (level) {
				case LogLevel.Debug:
					line += "[DEB]\t";
					lineColor = ConsoleColor.White;
					break;
				case LogLevel.Info:
					line += "[INF]\t";
					lineColor = ConsoleColor.Green;
					break;
				case LogLevel.Warning:
					line += "[WRN]\t";
					lineColor = ConsoleColor.Yellow;
					break;
				case LogLevel.Error:
					line += "[ERR]\t";
					lineColor = ConsoleColor.Red;
					break;
			}
			line += message;
			if (!Program.noColor) {
				Console.ForegroundColor = lineColor;
			}
			Console.WriteLine(message);

			if (!disposed) {
				textWriter.WriteLine(line);
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
