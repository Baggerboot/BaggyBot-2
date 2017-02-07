using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class ResetDb : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "resetdb";
		public override string Usage => "";
		public override string Description => "Completely reset the database, dropping and recreating all tables.";

		public override void Use(CommandArgs command)
		{
			StatsDatabase.Reset();
			command.Reply("the database has been reset successfully.");
		}
	}
}
