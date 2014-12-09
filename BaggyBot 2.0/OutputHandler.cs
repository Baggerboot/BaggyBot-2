using System.Linq;
using System.Text;

namespace BaggyBot.OutputHandler
{
	class Console
	{
		private static readonly StringBuilder lineBuffer = new StringBuilder();

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
