using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	class Snag : ICommand
	{
		private IrcInterface ircInterface;

		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Snag(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			switch (command.Args.Length) {
				case 0:
					ControlVariables.SnagNextLine = true;
					break;
				case 1:
					ControlVariables.SnagNextLineBy = command.Args[0];
					break;
				default:
					ircInterface.SendMessage(command.Channel, "Usage: -snag [nickname]");
					break;
			}
		}
	}
}
