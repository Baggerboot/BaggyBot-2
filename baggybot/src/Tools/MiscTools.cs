using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;

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

		/// <summary>
		/// Dynamically and recursively looks up properties on an object.
		/// For instance, the mapping { "TimeOfDay", "Hours" }, when performed on a DateTime object,
		/// will return the value of object.TimeOfDay.Hours.
		/// </summary>
		/// <param name="mapping">An array of property names that should be traversed</param>
		/// <param name="obj">The root object from which the first property should be looked up</param>
		/// <returns></returns>
		public static dynamic GetDynamic(string[] mapping, object obj)
		{
			var objType = obj.GetType();
			var propertyName = mapping[0];
			// If we're trying to look up an empty or null property on the current object,
			// it makes the most sense to simply return the current object.
			if (string.IsNullOrEmpty(propertyName)) return obj;
			var property = objType.GetProperty(propertyName);
			if (property == null) return null;
			if (mapping.Length == 1) return property.GetValue(obj);
			return GetDynamic(mapping.Skip(1).ToArray(), property.GetValue(obj));
		}

		public static JObject GetJson(string requestUri, string method = "GET")
		{
			var request = WebRequest.Create(requestUri);
			request.Method = method;
			using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
			{
				return JObject.Parse(reader.ReadToEnd());
			}
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
			const int peHeaderOffset = 60;
			const int linkerTimestampOffset = 8;
			var b = new byte[2048];
			Stream s = null;

			try
			{
				s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				s.Read(b, 0, 2048);
			}
			finally
			{
				s?.Close();
			}

			var i = BitConverter.ToInt32(b, peHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(b, i + linkerTimestampOffset);
			var dt = new DateTime(1970, 1, 1, 0, 0, 0);
			dt = dt.AddSeconds(secondsSince1970);
			dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
			return dt;
		}
	}
}
