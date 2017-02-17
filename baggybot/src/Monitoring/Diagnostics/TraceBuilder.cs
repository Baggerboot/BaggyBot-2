using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BaggyBot.Monitoring.Diagnostics
{
	internal static class TraceBuilder
	{
		private static Stopwatch sw;
		private static List<Snapshot> entries;

		public static void Start()
		{
			sw = new Stopwatch();
			entries = new List<Snapshot>(1000);
			sw.Start();
			Snapshot("timer-begin");
		}

		public static void Snapshot(string location)
		{
			var ticks = sw.ElapsedTicks;
			sw.Restart();

			var st = new StackTrace();
			var sf = st.GetFrame(1);
			var method = sf.GetMethod().Name;
			entries.Add(new Snapshot
			{
				SincePrevious = TimeSpan.FromTicks(ticks),
				Location = location,
				Method = method
			});
		}

		public static string GetResult()
		{
			var sb = new StringBuilder();
			var total = TimeSpan.Zero;
			foreach (var entry in entries)
			{
				total += entry.SincePrevious;
				sb.AppendLine(entry + $" (total: {total.TotalMilliseconds:0000.000})");
			}
			return sb.ToString();
		}
	}

	internal struct Snapshot
	{
		public TimeSpan SincePrevious { get; set; }
		public string Location { get; set; }
		public string Method { get; set; }

		public override string ToString()
		{
			return $"{Location} ({Method})".PadRight(50) + $"{SincePrevious.TotalMilliseconds:0000.000}ms";
		}
	}
}
