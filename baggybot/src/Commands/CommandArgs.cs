using System;
using IRCSharp.IRC;

namespace BaggyBot.Commands
{
	public struct CommandArgs
	{
		public string Command;
		public string[] Args;
		public IrcUser Sender;
		public string Channel;
		public string FullArgument;
		public readonly Action<string, string> ReplyCallback;

		public CommandArgs(string command, string[] args, IrcUser sender, string channel, string fullArgument, Action<string, string> replyCallback)
		{
			Command = command;
			Args = args;
			Sender = sender;
			Channel = channel;
			FullArgument = fullArgument;
			ReplyCallback = replyCallback;
		}

		public void Reply(string format, params object[] args)
		{
			var message = Sender.Nick + ", " + string.Format(format, args);
			ReplyCallback(Channel, message);
		}

		public void ReturnMessage(string format, params object[] args)
		{
			ReplyCallback(Channel, args.Length == 0 ? format : string.Format(format, args));
		}
	}
}
