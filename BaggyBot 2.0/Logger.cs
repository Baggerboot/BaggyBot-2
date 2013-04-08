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
		private const string filename = "test.txt";

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
			string format = String.Format("[{0}]\t{1}", level.ToString(), message);
			textWriter.WriteLine(format);
			Console.WriteLine(format);
			textWriter.Flush();
		}
		internal static void ClearLog()
		{
			textWriter.Close();
			File.Delete(filename);
			LoadLogFile();
		}
	}
}
