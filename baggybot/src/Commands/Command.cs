using System.Linq;
using BaggyBot.Database;
using BaggyBot.Formatting;
using BaggyBot.MessagingInterface;

namespace BaggyBot.Commands
{
	internal abstract class Command
	{
		public abstract PermissionLevel Permissions { get; }
		public abstract string Name { get; }
		public abstract string Usage { get; }
		public abstract string Description { get; }
		protected StatsDatabaseManager StatsDatabase => Client.StatsDatabase;
		public ChatClient Client { get; set; }

		public abstract void Use(CommandArgs c);

		public void InformUsage(CommandArgs cmd)
		{
			cmd.Reply($"usage: {Frm.M}{Bot.CommandIdentifiers.First()}{cmd.Command} {Usage}{Frm.M} -- {Description}");
		}
	}
}