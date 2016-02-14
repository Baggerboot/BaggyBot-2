using System;
using System.Linq;
using BaggyBot.Database;
using BaggyBot.DataProcessors;
using BaggyBot.Monitoring;
using IRCSharp;

namespace BaggyBot.Commands
{
	internal class Topics : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "[-d] [username] [channel]";
		public override string Description => "Find the topics associated with a given username in a given channel. Default values are the username of the sender and the channel the command was entered in. The -d flag will print additional debug info.";
		

		private void ShowTopics(string nick, string channel, CommandArgs command, bool showDebugInfo)
		{
			Logger.Log(this, "Showing topics for " + nick);
			var userId = command.Client.StatsDatabase.GetIdFromNick(nick);
			var topics = command.Client.StatsDatabase.FindTopics(userId, channel);

			if (topics == null)
			{
				command.Reply("could not find any IRC data by {0}. Did you spell their name correctly?", nick);
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

			command.Reply("words associated with {0}: {1}", nick, topicString);
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
				ShowTopics(command.Sender.Nick, command.Channel, command, showDebugInfo);
			}
			else if (command.Args.Length > 2)
			{
				command.ReturnMessage("Usage: -topics [nick]");
			}
			else if (command.Args.Length == 2)
			{
				ShowTopics(command.Args[0], command.Args[1], command, showDebugInfo);
			}
			else {
				ShowTopics(command.Args[0], command.Channel, command, showDebugInfo);
			}
		}
	}
}
