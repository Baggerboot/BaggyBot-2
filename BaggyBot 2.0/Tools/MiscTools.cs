using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	public static class MiscTools
	{
		public static void ConsoleWriteLine(string line, ConsoleColor color = ConsoleColor.Gray)
		{
			//var prev = Console.ForegroundColor;
			//Console.ForegroundColor = color;
			Console.WriteLine(line);
			//Console.ForegroundColor = prev;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string GetCurrentMethod()
		{
			var st = new StackTrace();
			var sf = st.GetFrame(1);

			return sf.GetMethod().Name;
		}

		public static string GetContentName(out string filename, out int num, string dirname, string extension, int depth)
		{
			var prefix = "/var/www/html/usercontent/" + dirname;

			var files = Directory.GetFiles(prefix).Where(s => s.EndsWith(extension)).OrderBy(s => s);
			num = 1;
			if (files.Count() != 0) {
				var name = files.Last();

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
			var x = 0.1;
			do {
				deltaX = (baseValue / Math.Pow(x, N - 1) - x) / N;
				x = x + deltaX;
			} while (Math.Abs(deltaX) > 0);
			return x;
		}

		public static DateTime RetrieveLinkerTimestamp()
		{
			var filePath = Assembly.GetCallingAssembly().Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;
			var b = new byte[2048];
			Stream s = null;

			try {
				s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				s.Read(b, 0, 2048);
			} finally {
				if (s != null) {
					s.Close();
				}
			}

			var i = BitConverter.ToInt32(b, c_PeHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
			var dt = new DateTime(1970, 1, 1, 0, 0, 0);
			dt = dt.AddSeconds(secondsSince1970);
			dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
			return dt;
		}

		public static string SerializeObject(object o)
		{
			if (!o.GetType().IsSerializable)
			{
				return null;
			}

			using (var stream = new MemoryStream())
			{
				new BinaryFormatter().Serialize(stream, o);
				return Convert.ToBase64String(stream.ToArray());
			}
		}
		public static object DeserializeObject(string str)
		{
			var bytes = Convert.FromBase64String(str);

			using (var stream = new MemoryStream(bytes))
			{
				return new BinaryFormatter().Deserialize(stream);
			}
		}
	}
}
