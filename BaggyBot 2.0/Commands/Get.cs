using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Get : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Get(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length != 2) {
				ircInterface.SendMessage(command.Channel, "Usage: -get <property> <key>");
				return;
			} switch (command.Args[0]) {
				case "uid":
					int uid = dataFunctionSet.GetIdFromNick(command.Args[1]);
					break;
				default:
					ircInterface.SendMessage(command.Channel, "That is not a valid property.");
					break;
			}

		}
	}
}
