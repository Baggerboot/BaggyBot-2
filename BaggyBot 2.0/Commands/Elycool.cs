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

		public Elycool(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(Command command)
		{
			ircInterface.SendMessage("#fofftopic", "Elystus is a cool guy.");

		}
	}
}
