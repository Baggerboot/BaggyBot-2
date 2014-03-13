using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Topics : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		private DataFunctionSet dataFunctionSet;

		public Topics(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		private void ShowTopics(string nick, string channel, Action<string, object[]> replyCallback, bool showDebugInfo)
		{
			Logger.Log("Showing topics for " + nick);
			int userId = dataFunctionSet.GetIdFromNick(nick);
			var topics = dataFunctionSet.FindTopics(userId, channel);

			if (topics == null) {
				replyCallback("Could not find any IRC data by {0}. Did you spell their name correctly?", new [] { nick });
				return;
			}

			string topicString;

			if (showDebugInfo) {
				topicString = string.Join(", ", topics.Take(20).Select(pair => string.Format("\x02{0}\x02 ({1}/{2}: {3:N2})", pair.Name, pair.UserCount, pair.GlobalCount, pair.Score)));
			} else {
				topicString = string.Join(", ", topics.Take(20).Select(pair => pair.Name));
			}

			

			replyCallback("Words associated with {0}: {1}", new [] {nick, topicString });
		}

		public void Use(CommandArgs command)
		{
			bool showDebugInfo = false;
			if (command.Args[0] == "-d") {
				command.Args = command.Args.Skip(1).ToArray();
				showDebugInfo = true;
			}
			if (command.Args.Length == 0) {
				ShowTopics(command.Sender.Nick, command.Channel, command.Reply, showDebugInfo);
			} else if (command.Args.Length > 2) {
				command.ReturnMessage("Usage: -topics [nick]");
			} else if(command.Args.Length == 2){
				ShowTopics(command.Args[0], command.Args[1], command.Reply, showDebugInfo);
			} else {
				ShowTopics(command.Args[0], command.Channel, command.Reply, showDebugInfo);
			}
		}
	}
}
