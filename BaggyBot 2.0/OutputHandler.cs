using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.OutputHandler
{
	class Console
	{
		private static StringBuilder lineBuffer = new StringBuilder();

		public static void WriteLine(string format, params object[] args)
		{

		}
		public static void Write(string format, params object[] args)
		{
			if (format.Contains('\n')) {

			}
			lineBuffer.Append(string.Format(format, args));
		}
	}
}
