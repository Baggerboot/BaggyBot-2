using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Elycool : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Elycool(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.SendMessage("#fofftopic", "Elystus is a cool guy.");
		}
	}
}
