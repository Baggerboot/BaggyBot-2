using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Say : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Say(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			string msg = "";
			command.Args.Skip(2).Select(param => msg += param);

			ircInterface.SendMessage(command.Args[0], msg);
		}
	}
}
