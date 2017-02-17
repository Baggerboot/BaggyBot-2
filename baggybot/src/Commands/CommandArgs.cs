using System;
using System.Linq;
using BaggyBot.MessagingInterface;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Plugins;

namespace BaggyBot.Commands
{
	internal struct CommandArgs
	{
		public string Command { get; }
		public string[] Args { get; }
		public ChatUser Sender { get; }
		public ChatChannel Channel { get; }
		public string FullArgument { get; }
		private readonly Func<string, MessageSendResult> replyCallback;
		private readonly Func<string, MessageSendResult> returnMessageCallback;

		/// <summary>
		/// Creates a new CommandArgs object, containing information about a command invocation.
		/// </summary>
		/// <param name="command">The name of the requested command.</param>
		/// <param name="args">An array of arguments to be passed to the command.</param>
		/// <param name="sender">The user who sent the command.</param>
		/// <param name="channel">The channel the command was sent in.</param>
		/// <param name="fullArgument">All arguments that were passed to the command, as a single string.</param>
		/// <param name="replyCallback">A callback method used to post a message to the channel
		/// the command was sent from, replying to the sender of the command.</param>
		/// <param name="returnMessageCallback">A callback methodd used to post a message to the channel
		/// the command was sent from.</param>
		public CommandArgs(string command, string[] args, ChatUser sender, ChatChannel channel, string fullArgument, Func<string, MessageSendResult> replyCallback, Func<string, MessageSendResult> returnMessageCallback)
		{
			Command = command;
			Args = args;
			Sender = sender;
			Channel = channel;
			FullArgument = fullArgument;
			this.replyCallback = replyCallback;
			this.returnMessageCallback = returnMessageCallback;
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
			var args = newCommand.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = newCommand.IndexOf(' ');
			return new CommandArgs(command, args, context.Sender, context.Channel, cmdIndex == -1 ? null : newCommand.Substring(cmdIndex + 1), context.replyCallback, context.returnMessageCallback);
		}

		public static CommandArgs FromPrevious(string newCommand, string newArguments, CommandArgs context)
		{
			var args = newArguments.Split(' ');
			return new CommandArgs(newCommand, args, context.Sender, context.Channel, newArguments, context.replyCallback, context.returnMessageCallback);
		}

		/// <summary>
		/// Generates a new CommandArgs object from an IRC message.
		/// Drops the commandPrefix from the beginning of the message, splits the
		/// rest of the message on spaces, using the first substring as the command
		/// name, and considers the rest to be arguments.
		/// </summary>
		/// <param name="commandPrefix">The command prefix matching this command.</param>
		/// <param name="ev">The IRC message from which the command should be
		/// constructed.</param>
		/// <returns>A CommandArgs object generated from the supplied ChatMessage.
		/// </returns>
		public static CommandArgs FromMessage(string commandPrefix, MessageEvent ev)
		{
			var line = ev.Message.Body.Substring(commandPrefix.Length);
			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			return new CommandArgs(command, args, ev.Message.Sender, ev.Message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1), ev.ReplyCallback, ev.ReturnMessageCallback);
		}

		/// <summary>
		/// Replies to the sender of a command, mentioning their nickname.
		/// The message will be sent to the same target the command was sent to.
		/// </summary>
		public MessageSendResult Reply(string format)
		{
			return replyCallback(format);
		}

		/// <summary>
		/// Sends a message to the target the command was sent to.
		/// </summary>
		public MessageSendResult ReturnMessage(string format)
		{
			return returnMessageCallback(format);
		}
	}
}
