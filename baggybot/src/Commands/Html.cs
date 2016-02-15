using System;

namespace BaggyBot.Commands
{
	internal class Html : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "[-h] <html code>";
		public override string Description => "Generate some HTML code. Use the -h switch to automatically add all the boilerplate tags so you can start entering the contents of the <body> immediately.";

		public override void Use(CommandArgs command)
		{
			throw new NotImplementedException("This command is currently not available.");
			// TODO: Reimplement the HTML command
			/*if (string.IsNullOrEmpty(command.FullArgument))
			{
				command.Reply("Usage: html [-h] <html code> - Use the -h switch to automatically add a doctype decoration, and opening and closing HTML and body tags");
				return;
			}

			var wrapBoilerplate = false;

			if (command.Args.Length > 0 && command.Args[0] == "-h")
			{
				wrapBoilerplate = true;
				command.FullArgument = command.FullArgument.Substring(3);
			}

			string filename;
			int fileId;
			using (var sw = new StreamWriter(MiscTools.GetContentName(out filename, out fileId, "html", ".html", 4)))
			{
				sw.WriteLine((wrapBoilerplate ? "<!DOCTYPE html><html><body>" : string.Empty) + command.FullArgument.Replace("<?php", string.Empty) + (wrapBoilerplate ? "</body></html>" : ""));
			}
			command.Reply("http://jgeluk.net/usercontent/html/{0}", filename);*/
		}
	}
}
