using System;
using System.Linq;
using BaggyBot.DataProcessors;
using IRCSharp.IRC;

namespace BaggyBot.Commands
{
	public struct CommandArgs
	{
		public string Command { get; private set; }
		// TODO: CommandArgs.Args should be made into a get-only accessor.
		public string[] Args { get; set; }
		public IrcUser Sender { get; private set; }
		public string Channel { get; private set; }
		// TODO: CommandArgs.FullArgument should be made into a get-only accessor.
		public string FullArgument { get; set; }
		//public Func<string, string, bool> ReplyCallback { get; private set; }

		public IrcInterface IrcInterface { get; private set; }

		public CommandArgs(string command, string[] args, IrcUser sender, string channel, string fullArgument, IrcInterface ircInterface)
		{
			Command = command;
			Args = args;
			Sender = sender;
			Channel = channel;
			FullArgument = fullArgument;
			IrcInterface = ircInterface;
			//ReplyCallback = replyCallback;
		}

		/// <summary>
		/// Generates a new CommandArgs object, using <paramref name="newCommand"/> as the command string
		/// (command + arguments) while using <paramref name="context"/> as the command context
		/// (sender, channel, IRC server, etc)
		/// </summary>
		/// <param name="newCommand"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static CommandArgs FromPrevious(string newCommand, CommandArgs context)
		{
			var line = newCommand.Substring(1);

			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			return new CommandArgs(command, args, context.Sender, context.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1), context.IrcInterface);
		}
		

		public static CommandArgs FromMessage(IrcMessage message)
		{
			var line = message.Message.Substring(1);

			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			return new CommandArgs(command, args, message.Sender, message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1), message.IrcInterface);
		}

		public bool Reply(string format, params object[] args)
		{
			var message = Sender.Nick + ", " + string.Format(format, args);
			return IrcInterface.SendMessage(Channel, message);
		}

		public bool ReturnMessage(string format, params object[] args)
		{
			return IrcInterface.SendMessage(Channel, args.Length == 0 ? format : string.Format(format, args));
		}
	}
}
