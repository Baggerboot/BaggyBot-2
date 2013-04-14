using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot.Commands
{
	struct CommandArgs
	{
		public string Command;
		public string[] Args;
		public IrcUser Sender;
		public string Channel;

		public CommandArgs(string command, string[] args, IrcUser sender, string channel)
		{
			Command = command;
			Args = args;
			Sender = sender;
			Channel = channel;
		}
	}
}
