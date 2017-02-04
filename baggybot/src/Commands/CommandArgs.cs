using System.Linq;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;

namespace BaggyBot.Commands
{
	public struct CommandArgs
	{
		public string Command { get; private set; }
		// TODO: CommandArgs.Args should be made into a get-only accessor.
		public string[] Args { get; set; }
		public ChatUser Sender { get;}
		public ChatChannel Channel { get;}
		public string FullArgument { get; }
		public Plugin Client { get; }

		/// <summary>
		/// Creates a new CommandArgs object, containing information about a command invocation.
		/// </summary>
		/// <param name="command">The name of the requested command.</param>
		/// <param name="args">An array of arguments to be passed to the command.</param>
		/// <param name="sender">The <see cref="ChatUser"/> who sent the command.</param>
		/// <param name="channel">The channel the command was sent in.</param>
		/// <param name="fullArgument">All arguments that were passed to the command, as a single string.</param>
		public CommandArgs(Plugin client, string command, string[] args, ChatUser sender, ChatChannel channel, string fullArgument)
		{
			Client = client;
			Command = command;
			Args = args;
			Sender = sender;
			Channel = channel;
			FullArgument = fullArgument;
			//ReplyCallback = replyCallback;
		}

		/// <summary>
		/// Generates a new CommandArgs object, using  <paramref 
		/// name="newCommand"/> as the command string (command + arguments)
		/// while using  <paramref name="context"/> as the command context
		/// (sender, channel, IRC server, etc)
		/// </summary>
		/// <param name="newCommand">The string from which the new command
		/// should be constructed.</param>
		/// <param name="context">The context (sender, channel, server, etc)
		/// that will be added to the newly created command.</param>
		/// <returns></returns>
		public static CommandArgs FromPrevious(string newCommand, CommandArgs context)
		{
			// TODO: this should probably be dropped
			var line = newCommand.Substring(1);

			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			return new CommandArgs(context.Client, command, args, context.Sender, context.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1));
		}
		
		public static CommandArgs FromPrevious(string newCommand, string newArguments, CommandArgs context)
		{
			var args = newArguments.Split(' ');
			return new CommandArgs(context.Client, newCommand, args, context.Sender, context.Channel, newArguments);
		}

		/// <summary>
		/// Generates a new CommandArgs object from an IRC message. Assumes that the
		/// first character of the message is the command identifier and removes it.
		/// Splits the message on spaces, using the first substring as the command
		/// name, and considers the rest to be arguments.
		/// </summary>
		/// <param name="message">The IRC message from which the command should be
		/// constructed.</param>
		/// <returns>A CommandArgs object generated from the supplied ChatMessage.
		/// </returns>
		public static CommandArgs FromMessage(ChatMessage message)
		{
			var line = message.Message.Substring(1);

			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			return new CommandArgs(message.Client, command, args, message.Sender, message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1));
		}

		/// <summary>
		/// Replies to the sender of a command, mentioning their nickname.
		/// The message will be sent to the same target the command was sent to.
		/// </summary>
		/// <param name="format">A format string, see <see cref="string.Format"/></param>
		/// <param name="args">An object array that contains zero or more objects
		/// to format, see <see cref="string.Format"/></param>
		/// <returns>A <see cref="MessageSendResult"/> containing information about
		/// the status of the message that was sent.</returns>
		public MessageSendResult Reply(string format, params object[] args)
		{
			var message = (Sender.Client.AtMention ? "@" : "") + Sender.AddressableName + ", " + string.Format(format, args);
			return Client.SendMessage(Channel, message);
		}

		/// <summary>
		/// Sends a message to the target the command was sent to.
		/// </summary>
		/// <param name="format">A format string, see <see cref="string.Format"/></param>
		/// <param name="args">An object array that contains zero or more objects
		/// to format, see <see cref="string.Format"/></param>
		/// <returns>A <see cref="MessageSendResult"/> containing information about
		/// the status of the message that was sent.</returns>
		public MessageSendResult ReturnMessage(string format, params object[] args)
		{
			return Client.SendMessage(Channel, args.Length == 0 ? format : string.Format(format, args));
		}
	}
}
