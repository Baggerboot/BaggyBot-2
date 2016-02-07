using System;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	internal class Get : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<property> [key]";
		public string Description => "Retrieves the value of a property, or the value of a key belonging to that property.";

		private readonly DataFunctionSet dataFunctionSet;
		private readonly IrcInterface ircInterface;

		public Get(DataFunctionSet df, IrcInterface ircInterface)
		{
			dataFunctionSet = df;
			this.ircInterface = ircInterface;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length > 2)
			{
				command.Reply("Usage: -get <property> <key>");
				return;
			}
			if (command.Args.Length == 2 && command.Args[0] == "-s")
			{
				// TODO: Allow settings lookup for new settings format
				// TODO: Disallow lookup of settings that should not be exposed
				throw new NotImplementedException("Dynamic YAML settings lookup is not supported yet.");
			}
			switch (command.Args[0])
			{
				case "uid":
					var nick = command.Args.Length > 1 ? command.Args[1] : command.Sender.Nick;
					var uid = dataFunctionSet.GetIdFromNick(nick);
					command.Reply("Your user Id is " + uid);
					break;
				case "users":
					if (command.Args.Length != 2)
					{
						command.Reply("usage: -get users <#channel>");
					}
					else {
						if (ircInterface.InChannel(command.Args[1]))
						{
							command.Reply("users in {0}: {1}", command.Args[1], string.Join(", ", ircInterface.GetUsers(command.Args[1])));
						}
						else {
							command.Reply("I am not in that channel");
						}
					}
					break;
				default:
					command.ReturnMessage("That is not a valid property.");
					break;
			}
		}
	}
}
