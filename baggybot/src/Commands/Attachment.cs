using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.CommandParsing;

namespace BaggyBot.Commands
{
	
	class Attachment : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "";
		public override string Usage => "";
		public override string Description => "";

		public override void Use(CommandArgs command)
		{
			var parser = new CommandParser(new Operation())
				.AddOperation("save", new Operation()
					.AddArgument("name")
					.AddArgument("url"))
				.AddOperation("delete", new Operation()
					.AddArgument("name"))
				.AddOperation("show", new Operation()
					.AddArgument("key")
					.AddRestArgument("value"));

			var cmd = parser.Parse(command.FullArgument);

			switch (cmd.OperationName)
			{
				case "save":


					SetName(cmd.Arguments["match"], cmd.Flags["uid"], cmd.RestArgument);
					command.Reply("Done");
					break;
				case "delete":
					SetCfg();
					break;
				case "show":

				default:
					InformUsage(command);
					break;
			}
		}
	}
}

