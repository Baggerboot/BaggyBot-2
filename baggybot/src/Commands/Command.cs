using System;
using System.Linq;
using BaggyBot.CommandParsing;
using BaggyBot.Database;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Commands
{
	internal abstract class Command : IDisposable
	{
		public abstract PermissionLevel Permissions { get; }
		public string PermissionName => "baggybot.commands." + Name;
		public abstract string Name { get; }
		public abstract string Usage { get; }
		public abstract string Description { get; }
		protected StatsDatabaseManager StatsDatabase => Client.StatsDatabase;
		public ChatClient Client { get; set; }

		public abstract void Use(CommandArgs cmd);

		public void InformUsage(CommandArgs cmd)
		{
			cmd.Reply($"usage: {Frm.M}{Bot.CommandIdentifiers.First()}{cmd.Command} {Usage}{Frm.M} -- {Description}");
		}

		public virtual void Dispose()
		{
			
		}

		public bool HasPermission(CommandArgs invocation)
		{
			var dbPermission = Client.Permissions.CheckPermission(invocation.Sender, invocation.Channel, PermissionName);

			// If the permissions manager denies or grants permission for this command usage, we should honour that.
			if (dbPermission.HasValue) return dbPermission.Value;

			// If the permissions manager does not have anything to say about this, we can rely on built-in permissions.
			if (Permissions == PermissionLevel.All) return true;
			try
			{
				return Client.IsOperator(invocation.Sender);
			}
			catch (Exception)
			{
				invocation.Reply("I am unable to validate your account.");
				return false;
			}
		}
	}

	internal abstract class ParameterisedCommand : Command
	{
		public abstract CommandParser CommandParser { get; }

		public abstract void Use(CommandArgs cmd, OperationResult result);

		public sealed override void Use(CommandArgs cmdArgs)
		{
			var result = CommandParser.Parse(cmdArgs.FullArgument);
			Use(cmdArgs, result);
		}
	}
}