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
		private DataFunctionSet dataFunctionSet;

		public NickServ(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(Command c)
		{
			string ns = dataFunctionSet.GetNickserv(dataFunctionSet.GetIdFromUser(c.Sender));
			ircInterface.SendMessage(c.Channel, "Your NickServ username is " + ns);
		}
	}
}
