using System;
using BaggyBot.CommandParsing;

namespace BaggyBot.Commands
{
	internal class Get : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<property> [key]";
		public override string Description => "Retrieves the value of a property, or the value of a key belonging to that property.";

		public override void Use(CommandArgs command)
		{
			var cmdParser = new CommandParser(new Operation())
				.AddOperation("cfg", new Operation()
					.AddArgument("config-key", null))
				.AddOperation("uid", new Operation()
					.AddArgument("user", command.Sender.Nick))
				.AddOperation("users", new Operation()
					.AddArgument("channel", command.Channel));

			OperationResult result;
			try
			{
				result = cmdParser.Parse(command.FullArgument);
			}
			catch (InvalidCommandException e)
			{
				command.ReturnMessage(e.Message);
				return;
			}

			switch (result.OperationName)
			{
				case "default":
					InformUsage(command);
					break;
				case "cfg":
					// TODO: Allow settings lookup for new settings format, disallow lookup of settings that should not be exposed
					throw new NotImplementedException("Dynamic YAML settings lookup is not supported yet.");
				case "uid":
					var uid = command.Client.StatsDatabase.GetIdFromNick(result.Arguments["user"]);
					command.Reply("Your user Id is " + uid);
					break;
				case "users":
					var channel = result.Arguments["channel"];
					
					if (command.Client.InChannel(channel))
					{
						var ircChannel = command.Client.GetChannel(channel);
						command.Reply($"users in {channel}: {string.Join(", ", ircChannel.UserCount)}");
					}
					else
					{
						command.Reply("I'm not in that channel.");
					}
					break;
			}
		}
	}
}
