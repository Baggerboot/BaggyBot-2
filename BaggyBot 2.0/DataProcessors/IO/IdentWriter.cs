using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BaggyBot.DataProcessors
{
	class IdentWriter
	{
		private const string identfile = @"/home/baggybotuser/.oidentd.conf";

		public void Write(int port, string ident)
		{
			Logger.Log("Writing ident '{0}' and port '{1}' to identfile", LogLevel.Debug, true, ident, port);
			using (StreamWriter sw = new StreamWriter(new FileStream(identfile, FileMode.Create, FileAccess.ReadWrite))) {
				sw.WriteLine("lport " + port + " { reply \"" + ident + "\" }");
			}
		}
	}
}
