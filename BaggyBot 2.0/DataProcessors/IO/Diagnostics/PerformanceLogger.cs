using System;
using System.Collections.Generic;
using System.IO;

namespace BaggyBot
{
	class PerformanceLogger : IDisposable
	{
		private readonly StreamWriter sw;
		public List<PerformanceObject> PerformanceLog
		{
			get;
			private set;
		}

		public PerformanceLogger(string filename)
		{
			PerformanceLog = new List<PerformanceObject>();
			sw = new StreamWriter(filename);
			sw.WriteLine("private.memory, channels.count, users.count");
		}

		public void Log(long memSize, int channelCount, int userCount)
		{
			PerformanceLog.Add(new PerformanceObject()
			{
				MemorySize = memSize,
				ChannelCount = channelCount,
				UserCount = userCount
			});
			sw.WriteLine("{0}, {1}, {2}", memSize, channelCount, userCount);
			sw.Flush();
		}

		public void Dispose()
		{
			sw.Close();
			sw.Dispose();
		}
	}
}
