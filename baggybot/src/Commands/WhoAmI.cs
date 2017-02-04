using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class WhoAmI : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "";
		public override string Description => "Displays some information about your message.";

		public override void Use(CommandArgs command)
		{
			var user = command.Sender;
			var sb = new StringBuilder();
			if (user.Name == null)
			{
				sb.Append($"Your nickname appears to be {user.Nickname}.");
			}
			else
			{
				sb.Append($"Your nickname appears to be {user.Nickname}, though you prefer to call yourself {user.Name}.");
			}
			sb.Append(
				$"Your unique userID is {user.UniqueId}, and you're posting in the channel \"{command.Channel.Name}\" ({command.Channel.Identifier}).");
			command.ReturnMessage(sb.ToString());
		}
	}
}
