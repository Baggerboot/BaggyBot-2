using System;
using System.Linq;
using BaggyBot.Database;
using BaggyBot.DataProcessors;
using BaggyBot.Monitoring;

namespace BaggyBot.Commands
{
	internal class Topics : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "[-d] [username] [channel]";
		public override string Description => "Find the topics associated with a given username in a given channel. Default values are the username of the sender and the channel the command was entered in. The -d flag will print additional debug info.";

		private readonly DataFunctionSet dataFunctionSet;

		public Topics(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		private void ShowTopics(string nick, string channel, Action<string, object[]> replyCallback, bool showDebugInfo)
		{
			Logger.Log(this, "Showing topics for " + nick);
			var userId = dataFunctionSet.GetIdFromNick(nick);
			var topics = dataFunctionSet.FindTopics(userId, channel);

			if (topics == null)
			{
				replyCallback("Could not find any IRC data by {0}. Did you spell their name correctly?", new object[] { nick });
				return;
			}

			string topicString;

			if (showDebugInfo)
			{
				topicString = string.Join(", ", topics.Take(20).Select(pair => $"\x02{pair.Name}\x02 ({pair.UserCount}/{pair.GlobalCount}: {pair.Score:N2})"));
			}
			else {
				topicString = string.Join(", ", topics.Take(20).Select(pair => pair.Name));
			}

			replyCallback("words associated with {0}: {1}", new object[] { nick, topicString });
		}

		public override void Use(CommandArgs command)
		{
			var showDebugInfo = false;
			if (command.Args[0] == "-d")
			{
				command.Args = command.Args.Skip(1).ToArray();
				showDebugInfo = true;
			}
			if (command.Args.Length == 0)
			{
				ShowTopics(command.Sender.Nick, command.Channel, command.Reply, showDebugInfo);
			}
			else if (command.Args.Length > 2)
			{
				command.ReturnMessage("Usage: -topics [nick]");
			}
			else if (command.Args.Length == 2)
			{
				ShowTopics(command.Args[0], command.Args[1], command.Reply, showDebugInfo);
			}
			else {
				ShowTopics(command.Args[0], command.Channel, command.Reply, showDebugInfo);
			}
		}
	}
}
