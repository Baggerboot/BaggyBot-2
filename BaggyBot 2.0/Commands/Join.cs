using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.Tools;
using BaggyBot.DataProcessors;


namespace BaggyBot.Commands
{
	class Join : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Join(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 1) {
				ircInterface.JoinChannel(command.Args[0]);
			} else {
				command.Reply("Usage: -join <channel>");
			}
		}
	}
}
