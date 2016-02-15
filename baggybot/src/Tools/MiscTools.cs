using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	public static class MiscTools
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static string GetCurrentMethod()
		{
			var st = new StackTrace();
			var sf = st.GetFrame(1);

			return sf.GetMethod().Name;
		}

		public static double NthRoot(double baseValue, int n)
		{
			if (n == 1)
				return baseValue;
			double deltaX;
			var x = 0.1;
			do
			{
				deltaX = ((baseValue / Math.Pow(x, n - 1)) - x) / n;
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

			try
			{
				s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				s.Read(b, 0, 2048);
			}
			finally
			{
				if (s != null)
				{
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
