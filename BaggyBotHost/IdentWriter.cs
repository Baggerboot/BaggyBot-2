using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BaggyBotHost
{
	class IdentWriter
	{
		private string identfile = @"/home/baggybotuser/.oidentd.conf";

		public void Write(int port, string ident)
		{
			if (Settings.Instance["enable_oidentd"] == "true") {
				var settingsIdentFile = Settings.Instance["identfile_location"];
				if (!string.IsNullOrWhiteSpace(Settings.Instance["identfile_location"])) {
					Logger.Log("Using alternate identfile location");
					identfile = settingsIdentFile;
				}
				Logger.Log("Writing ident '{0}' and port '{1}' to identfile", LogLevel.Host, true, ident, port);
				using (StreamWriter sw = new StreamWriter(new FileStream(identfile, FileMode.Create, FileAccess.ReadWrite))) {
					sw.WriteLine("lport " + port + " { reply \"" + ident + "\" }");
				}
			} else {
				Logger.Log("Oidentd integration disabled. Ident file will not be written to.");
			}
		}
	}
}
