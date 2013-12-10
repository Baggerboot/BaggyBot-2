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
			string prefix = "/var/www/html/usercontent/html";
			//string prefix = ".";

			bool wrapBoilerplate = false;

			if (command.Args.Length > 0 && command.Args[0] == "-h") {
				wrapBoilerplate = true;
				command.FullArgument = command.FullArgument.Substring(3);
			}

			var files = Directory.GetFiles(prefix).Where(s => s.EndsWith(".html")).OrderBy(s => s);
			int num = 1;
			if(files.Count() != 0){
				string name = files.Last();
				
				name = name.Split('/').Last();
				name = name.Substring(0, 4);
				num = int.Parse(name);
				num++;
			}
			using (StreamWriter sw = new StreamWriter(prefix + "/" + num.ToString("D4") + ".html")) {
				sw.WriteLine((wrapBoilerplate ? "<!DOCTYPE html><html><body>" : "") + command.FullArgument.Replace("<?php", "") + (wrapBoilerplate ? "</body></html>" : ""));
			}
			ircInterface.SendMessage(command.Channel, string.Format("{0}, http://jgeluk.net/usercontent/html/{1}.html", command.Sender.Nick, num.ToString("D4")));
		}
	}
}
