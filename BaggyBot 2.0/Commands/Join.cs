using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Join : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;

		public Join(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(Command command)
		{
			if (dataFunctionSet.GetNickserv(dataFunctionSet.GetIdFromUser(command.Sender)).Equals("Baggerboot")) {
				if (command.Args.Length == 1) {
					ircInterface.JoinChannel(command.Args[0]);
				} else {
					ircInterface.SendMessage(command.Channel, "Usage: -join <channel>");
				}
			} else {
				ircInterface.SendMessage(command.Channel, "You are not authorized to use this command");
			}
		}
	}
}
