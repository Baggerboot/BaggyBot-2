using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Say : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }
		private IrcInterface ircInterface;

		public Say(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command){
			string msg = String.Join(" ", command.Args.Skip(1));
			Logger.Log("Saying: " + msg);

			ircInterface.SendMessage(command.Args[0], msg);
		}
	}
}
