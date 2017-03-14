using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.CommandParsing;
using BaggyBot.Commands;
using BaggyBot.Database.Model;
using BaggyBot.MessagingInterface;
using BaggyBot.Permissions;
using BaggyBot.Tools;
using SlackAPI;

namespace BaggyBot.Commands
{
	class SetPermissions : ParameterisedCommand
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "perm";
		public override string Usage => "[add] | [remove] | [find] | [test]";
		public override string Description => "Manage permission entries.";

		public override CommandParser CommandParser { get; }

		public SetPermissions()
		{
			CommandParser = new CommandParser(new Operation())
				.AddOperation(
					"add",
					new Operation()
						.AddArgument("name")
						.AddArgument("action")
						.AddKey("channel", 'c')
						.AddKey("channel-id", 'C')
						.AddKey("user-name", 'u')
						.AddKey("user-group", 'g')
						.AddKey("perm-name", 'p')
						.AddKey("perm-group", 'P')
						.AddFlag("disabled"))
				.AddOperation(
					"addgroup",
					new Operation()
						.AddArgument("group-name"))
				.AddOperation(
					"remove",
					new Operation()
						.AddArgument("name"))
				.AddOperation(
					"find",
					new Operation()
						.AddKey("query", 'q')
						.AddKey("channel", 'c')
						.AddKey("channel-id", 'C')
						.AddKey("user", 'u')
						.AddKey("user-group", 'g')
						.AddKey("perm-name", 'p')
						.AddKey("perm-group", 'P')
						.AddKey("enabled-only", false, 'e')
						.AddKey("max-results", 3, 'n'))
				.AddOperation(
					"test",
					new Operation()
						.AddArgument("perm-name")
						.AddKey("channel", 'c')
						.AddKey("channel-id", 'C')
						.AddKey("user-name", 'u'));
		}

		public override void Use(CommandArgs cmdArgs, OperationResult cmd)
		{
			switch (cmd.OperationName)
			{
				case "add":
					Add(cmdArgs, cmd);
					break;
				case "remove":
					Remove(cmdArgs, cmd);
					break;
				case "find":
					Find(cmdArgs, cmd);
					break;
				case "test":
					Test(cmdArgs, cmd);
					break;
			}
		}

		private void Test(CommandArgs cmdArgs, OperationResult cmd)
		{
			var permName = cmd.Arguments["perm-name"];
			var userName = cmd.Keys["user-name"];
			var channelId = cmd.Keys["channel-id"];
			var channelName = cmd.Keys["channel"];

			var user = userName == null ? cmdArgs.Sender : Client.FindUser(userName);
			var channel = channelName != null
				? Client.FindChannel(channelName)
				: channelId != null
					? Client.GetChannel(channelId)
					: cmdArgs.Channel;

			var entries = Client.Permissions.GetPermissionEntries(user, channel, new PermNode(permName));

			if (entries.Length == 0)
			{
				cmdArgs.Reply($"no applicable permission entries found for {user} in {channel}.");
			}
			else
			{
				cmdArgs.Reply($"most relevant permission entry: {StatsDatabase.DescribePermissionEntry(entries.First())}");
			}
		}

		private void Add(CommandArgs cmdArgs, OperationResult cmd)
		{
			var channelId = cmd.Keys["channel-id"];
			var channelName = cmd.Keys["channel"];
			if (channelName != null) channelId = Client.FindChannel(channelName).Identifier;
			// This might seem pointless, but it ensures that the channel ID is valid
			if (channelId != null) channelId = Client.GetChannel(channelId).Identifier;

			var action = (PermissionValue) Enum.Parse(typeof(PermissionValue), cmd.Arguments["action"].ToPascalCase());

			UserGroup userGroup = null;
			var group = cmd.Keys["user-group"];
			var name = cmd.Keys["user-name"];
			if (group != null) userGroup = StatsDatabase.GetUserGroup(group);
			if (name != null)
				userGroup = StatsDatabase.GetSingleUserGroup(StatsDatabase.MapUser(Client.FindUser(name)));

			PermissionGroup permGroup = null;
			group = cmd.Keys["perm-group"];
			name = cmd.Keys["perm-name"];
			if (group != null) permGroup = StatsDatabase.GetPermissionGroup(group);
			if (name != null) permGroup = StatsDatabase.GetSinglePermissionGroup(name);

			var entry = new PermissionEntry
			{
				Name = cmd.Arguments["name"],
				Enabled = !cmd.Flags["disabled"],
				ChannelId = channelId,
				UserGroup = userGroup?.Id,
				PermissionGroup = permGroup?.Id,
				Action = action
			};
			StatsDatabase.AddPermissionEntry(entry);
			cmdArgs.ReturnMessage($"Entry added: {StatsDatabase.DescribePermissionEntry(entry)}.");
		}

		private void Remove(CommandArgs cmdArgs, OperationResult cmd)
		{
		}

		private void Find(CommandArgs cmdArgs, OperationResult cmd)
		{
			var query = cmd.Keys["query"];
			var channelId = cmd.Keys["channel-id"];
			var channelName = cmd.Keys["channel"];
			var userGroup = cmd.Keys["user-group"];
			var permGroup = cmd.Keys["perm-group"];
			var userName = cmd.Keys["user"];
			var permName = cmd.Keys["perm-name"];
			var enabledOnly = cmd.GetKey<bool>("enabled-only");
			var maxResults = cmd.GetKey<int>("max-results");

			var user = userName == null ? null : Client.FindUser(userName);
			var channel = channelName != null
				? Client.FindChannel(channelName)
				: channelId != null
					? Client.GetChannel(channelId)
					: cmdArgs.Channel;

			var permissions = Client.Permissions.FindPermissionEntries(query, userGroup, permGroup, user.DbUser, new PermNode(permName), channel, enabledOnly);
			if (permissions.Length == 0)
			{
				cmdArgs.Reply("no matching permission entries found.");
			}
			foreach (var entry in permissions.Take(maxResults))
			{
				cmdArgs.ReturnMessage(StatsDatabase.DescribePermissionEntry(entry));
			}
			if (permissions.Length - maxResults > 0)
			{
				cmdArgs.ReturnMessage($"...and {permissions.Length - maxResults} more.");
			}
		}
	}
}