using System.Linq;
using BaggyBot.MessagingInterface;
using IRCSharp;

namespace BaggyBot.Commands
{
	public struct CommandArgs
	{
		public string Command { get; private set; }
		// TODO: CommandArgs.Args should be made into a get-only accessor.
		public string[] Args { get; set; }
		public IrcUser Sender { get;}
		public string Channel { get;}
		// TODO: CommandArgs.FullArgument should be made into a get-only accessor.
		public string FullArgument { get; set; }
		public IrcClientWrapper Client => Sender.Client;

		public CommandArgs(string command, string[] args, IrcUser sender, string channel, string fullArgument)
		{
			Command = command;
			Args = args;
			Sender = sender;
			Channel = channel;
			FullArgument = fullArgument;
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
			return new CommandArgs(command, args, context.Sender, context.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1));
		}
		

		public static CommandArgs FromMessage(IrcMessage message)
		{
			var line = message.Message.Substring(1);

			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			return new CommandArgs(command, args, message.Sender, message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1));
		}

		public MessageSendResult Reply(string format, params object[] args)
		{
			var message = Sender.Nick + ", " + string.Format(format, args);
			return Sender.Client.SendMessage(Channel, message);
		}

		public MessageSendResult ReturnMessage(string format, params object[] args)
		{
			return Sender.Client.SendMessage(Channel, args.Length == 0 ? format : string.Format(format, args));
		}
	}
}
