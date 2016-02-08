using System;
using BaggyBot.CommandParsing;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	internal class Get : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<property> [key]";
		public override string Description => "Retrieves the value of a property, or the value of a key belonging to that property.";

		private readonly DataFunctionSet dataFunctionSet;
		private readonly IrcInterface ircInterface;

		public Get(DataFunctionSet df, IrcInterface ircInterface)
		{
			dataFunctionSet = df;
			this.ircInterface = ircInterface;
		}

		public override void Use(CommandArgs command)
		{
			var cmdParser = new CommandParser(new Operation())
				.AddOperation("cfg", new Operation()
					.AddArgument("config-key", null))
				.AddOperation("uid", new Operation()
					.AddArgument("user", command.Sender.Nick))
				.AddOperation("users", new Operation()
					.AddArgument("channel", command.Channel));

			var result = cmdParser.Parse(command.FullArgument);

			switch (result.OperationName)
			{
				case "default":
					InformUsage(command);
					break;
				case "cfg":
					// TODO: Allow settings lookup for new settings format
					// TODO: Disallow lookup of settings that should not be exposed
					throw new NotImplementedException("Dynamic YAML settings lookup is not supported yet.");
					break;
				case "uid":
					var uid = dataFunctionSet.GetIdFromNick(result.Arguments["user"]);
					command.Reply("Your user Id is " + uid);
					break;
				case "users":
					var channel = result.Arguments["channel"];
					if (ircInterface.InChannel(channel))
					{
						var users = ircInterface.GetUsers(channel);
						command.Reply($"users in {channel}: {string.Join(", ", users)}");
					}
					else
					{
						command.Reply($"I'm not in that channel.");
					}
					break;
			}
		}
	}
}
