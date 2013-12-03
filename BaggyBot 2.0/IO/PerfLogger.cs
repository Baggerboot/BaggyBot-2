using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BaggyBot
{
	class PerfLogger : IDisposable
	{
		private StreamWriter sw;

		public PerfLogger(string filename)
		{
			sw = new StreamWriter(filename);
			sw.WriteLine("private.memory, channels.count, users.count");
		}

		public void Log(long memSize, int channelCount, int userCount)
		{
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
