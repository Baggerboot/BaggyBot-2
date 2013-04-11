using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Crash : ICommand
	{
		private IrcInterface ircInterface;

		public Crash(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(Command command)
		{

		}
	}
}
