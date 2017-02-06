using System.Linq;
using BaggyBot.CommandParsing;
using BaggyBot.Configuration;
using BaggyBot.EmbeddedData;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	internal class Get : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<property> [key]";
		public override string Description => "Retrieves the value of a property, or the value of a key belonging to that property. Valid properties: [cfg, uid, users, channel]";

		public override void Use(CommandArgs command)
		{
			var cmdParser = new CommandParser(new Operation())
				.AddOperation("cfg", new Operation()
					.AddArgument("config-key"))
				.AddOperation("uid", new Operation()
					.AddArgument("user", command.Sender.Nickname))
				.AddOperation("users", new Operation()
					.AddArgument("channel", command.Channel.Name))
				.AddOperation("channel", new Operation()
					.AddArgument("channel-id", command.Channel.Identifier));

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
					GetCfg(command, result);
					break;
				case "channel":
					GetChannel(command, result);
					break;
				case "uid":
					GetUid(command, result);
					break;
				case "users":
					GetUsers(command, result);
					break;
			}
		}

		private void GetCfg(CommandArgs command, OperationResult result)
		{
			if (!UserTools.Validate(command.Sender))
			{
				command.Reply(Messages.CmdNotAuthorised);
				return;
			}
			var key = result.Arguments["config-key"];
			var value = MiscTools.GetDynamic(key.Split('.').Select(k => k.ToPascalCase()).ToArray(), ConfigManager.Config);
			command.Reply($"{(string.IsNullOrEmpty(key) ? "config" : "config." + key)} = {value}");
		}

		private void GetUid(CommandArgs command, OperationResult result)
		{
			var users = command.Client.StatsDatabase.GetUsersByNickname(result.Arguments["user"]);
			if (users.Length == 0)
				command.Reply($"I don't know a user with {result.Arguments["user"]} as their primary name");
			else
				command.Reply($"the user Id belonging to {result.Arguments["user"]} is {users[0].Id}");
		}

		private void GetUsers(CommandArgs command, OperationResult result)
		{
			var channel = result.Arguments["channel"];
			if (command.Client.InChannel(command.Client.FindChannel(channel)))
			{
				var ircChannel = command.Client.FindChannel(channel);
				command.Reply($"users in {channel}: {string.Join(", ", ircChannel.Users.Count)}");
			}
			else
			{
				command.Reply("I'm not in that channel.");
			}
		}

		private void GetChannel(CommandArgs command, OperationResult result)
		{
			var id = result.Arguments["channel-id"];
			var channel = command.Client.GetChannel(id);
			command.Reply($"{id} maps to {channel.Name} {(channel.IsPrivateMessage ? "(private message)" : "")}");
		}
	}
}
