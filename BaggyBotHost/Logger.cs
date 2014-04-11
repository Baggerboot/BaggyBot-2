﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BaggyBotHost
{
	public enum LogLevel
	{
		Host
	}

	public delegate void LogEvent(string message, LogLevel level);

	public static class Logger
	{
		private static string prefix = string.Empty;

		public static void SetPrefix(string prefix)
		{
			Logger.prefix = prefix;
		}

		public static void ClearPrefix()
		{
			Logger.prefix = string.Empty;
		}

		public static void Log(string message, params object[] format)
		{
			if (format.Length != 0) {
				message = String.Format(message, format);
			}
			StringBuilder lineBuilder = new StringBuilder();
			ConsoleColor lineColor = ConsoleColor.Gray;

			lineBuilder.Append("[HST]\t");
			lineColor = ConsoleColor.DarkYellow;

			lineBuilder.Append(prefix);
			lineBuilder.Append(message);

			WriteToConsole(lineColor, lineBuilder);

			lineBuilder.Insert(0, DateTime.Now.ToString("[MMM dd - HH:mm:ss.fff]\t"));
		}

		private static void WriteToConsole(ConsoleColor lineColor, StringBuilder lineBuilder)
		{
			var prevColor = Console.ForegroundColor;
			Console.ForegroundColor = lineColor;
			Console.WriteLine(lineBuilder.ToString());
			Console.ForegroundColor = prevColor;
		}
	}
}
