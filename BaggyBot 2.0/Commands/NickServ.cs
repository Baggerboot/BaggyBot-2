using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class NickServ : ICommand
	{
		private IrcInterface ircInterface;

		public NickServ(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(Command c)
		{

		}
	}
}
