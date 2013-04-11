using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot.Commands
{
	struct Command
	{
		public string Primary;
		public string Args;
		public IrcUser Sender;

		public Command(string command, string args, IrcUser sender)
		{
			Primary = command;
			Args = args;
			Sender = sender;
		}
	}
}
