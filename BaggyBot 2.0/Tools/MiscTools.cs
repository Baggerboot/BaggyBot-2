using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	public static class MiscTools
	{
		public static void ConsoleWriteLine(string line, ConsoleColor color = ConsoleColor.Gray)
		{
			var prev = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(line);
			Console.ForegroundColor = prev;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string GetCurrentMethod()
		{
			StackTrace st = new StackTrace();
			StackFrame sf = st.GetFrame(1);

			return sf.GetMethod().Name;
		}

		public static string GetContentName(out string filename, out int num, string dirname, string extension, int depth)
		{
			string prefix = "/var/www/html/usercontent/" + dirname;

			var files = Directory.GetFiles(prefix).Where(s => s.EndsWith(extension)).OrderBy(s => s);
			num = 1;
			if (files.Count() != 0) {
				string name = files.Last();

				name = name.Split('/').Last();
				name = name.Substring(0, depth);
				num = int.Parse(name);
				num++;
			}
			filename = num.ToString("D"+depth) + extension;
			return prefix + "/" + filename;
		}

		public static double NthRoot(double baseValue, int N)
		{
			if (N == 1)
				return baseValue;
			double deltaX;
			double x = 0.1;
			do {
				deltaX = (baseValue / Math.Pow(x, N - 1) - x) / N;
				x = x + deltaX;
			} while (Math.Abs(deltaX) > 0);
			return x;
		}

		public static DateTime RetrieveLinkerTimestamp()
		{
			string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;
			byte[] b = new byte[2048];
			System.IO.Stream s = null;

			try {
				s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				s.Read(b, 0, 2048);
			} finally {
				if (s != null) {
					s.Close();
				}
			}

			int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
			int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
			dt = dt.AddSeconds(secondsSince1970);
			dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
			return dt;
		}
	}
}
