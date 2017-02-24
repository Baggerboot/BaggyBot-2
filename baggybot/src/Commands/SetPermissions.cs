using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.CommandParsing;
using BaggyBot.Commands;
using BaggyBot.Database.Model;
using BaggyBot.MessagingInterface;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	class SetPermissions : ParameterisedCommand
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "perm";
		public override string Usage => "";
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
					"remove",
					new Operation()
						.AddArgument("name"))
				.AddOperation(
					"find",
					new Operation()
						.AddKey("query", 'q')
						.AddKey("channel", 'c')
						.AddKey("channel-id", 'C')
						.AddKey("user-group", 'g')
						.AddKey("perm-name", 'p')
						.AddKey("perm-group", 'P'))
						.AddOperation(
				"test",
				new Operation()
				.AddArgument("perm-name"));
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
			}
		}

		private void Add(CommandArgs cmdArgs, OperationResult cmd)
		{
			var channelId = cmd.Keys["channel-id"];
			var channelName = cmd.Keys["channel"];
			if (channelName != null) channelId = Client.FindChannel(channelName).Identifier;
			// This might seem pointless, but it ensures that the channel ID is valid
			if (channelId != null) channelId = Client.GetChannel(channelId).Identifier;

			var action = (PermissionValue)Enum.Parse(typeof(PermissionValue), cmd.Arguments["action"].ToPascalCase());

			UserGroup userGroup = null;
			var group = cmd.Keys["user-group"];
			var name = cmd.Keys["user-name"];
			if (group != null) userGroup = StatsDatabase.GetUserGroup(group);
			if (name != null) userGroup = StatsDatabase.GetSingleUserGroup(StatsDatabase.MapUser(Client.FindUser(name)));

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
			cmdArgs.ReturnMessage($"Permission applied: {StatsDatabase.DescribePermissionEntry(entry)}.");
		}

		private void Remove(CommandArgs cmdArgs, OperationResult cmd)
		{

		}

		private void Find(CommandArgs cmdArgs, OperationResult cmd)
		{

		}
	}
}
