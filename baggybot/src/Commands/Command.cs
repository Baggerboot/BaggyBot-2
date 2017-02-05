using System.Linq;
using BaggyBot.Formatting;

namespace BaggyBot.Commands
{
	public abstract class Command
	{
		public abstract PermissionLevel Permissions { get; }
		public abstract string Usage { get; }
		public abstract string Description { get; }

		public abstract void Use(CommandArgs c);

		public void InformUsage(CommandArgs cmd)
		{
			cmd.Reply($"usage: {Frm.C}{Bot.CommandIdentifiers.First()}{cmd.Command} {Usage}{Frm.C} -- {Description}");
		}
	}
}