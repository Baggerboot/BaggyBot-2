using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BaggyBot.Commands
{
	class Html : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Html(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			if (string.IsNullOrEmpty(command.FullArgument)) {
				ircInterface.SendMessage(command.Channel, "Usage: html [-h] <html code> - Use the -h switch to automatically add a doctype decoration, and opening and closing HTML and body tags");
			}
			//string prefix = ".";

			bool wrapBoilerplate = false;

			if (command.Args.Length > 0 && command.Args[0] == "-h") {
				wrapBoilerplate = true;
				command.FullArgument = command.FullArgument.Substring(3);
			}

			string filename;
			int fileId;
			using (StreamWriter sw = new StreamWriter(Tools.MiscTools.GetContentName(out filename, out fileId, "html", ".html", 4))) {
				sw.WriteLine((wrapBoilerplate ? "<!DOCTYPE html><html><body>" : "") + command.FullArgument.Replace("<?php", "") + (wrapBoilerplate ? "</body></html>" : ""));
			}
			ircInterface.SendMessage(command.Channel, string.Format("{0}, http://jgeluk.net/usercontent/html/{1}", command.Sender.Nick, filename));
		}
	}
}
