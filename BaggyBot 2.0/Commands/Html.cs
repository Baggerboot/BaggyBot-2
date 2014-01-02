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
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public void Use(CommandArgs command)
		{
			if (string.IsNullOrEmpty(command.FullArgument)) {
				command.Reply("Usage: html [-h] <html code> - Use the -h switch to automatically add a doctype decoration, and opening and closing HTML and body tags");
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
			command.Reply("http://jgeluk.net/usercontent/html/{0}", filename);
		}
	}
}
