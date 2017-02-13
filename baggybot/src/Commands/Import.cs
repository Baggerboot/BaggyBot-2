using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.CommandParsing;
using BaggyBot.MessagingInterface;
using Curse.NET.Model;
using Microsoft.Scripting;

namespace BaggyBot.Commands
{
	class Import : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "import";
		public override string Usage => "[<-n|--channel-name> channel][<-c|--channel-id> channel-id][-m|--map-users][<-f|--file-name> file]";
		public override string Description => "Imports messages for the given channel, or the current channel if no channel is specified.";

		public override void Use(CommandArgs command)
		{
			// TODO: implement map-users
			var parser = new CommandParser(new Operation()
				.AddKey("channel-name", null, 'c')
				.AddKey("channel-id", command.Channel.Identifier, 'C')
				.AddKey("file-name", null, 'f'));

			var result = parser.Parse(command.FullArgument);
			var channelName = result.Keys["channel-name"];
			var channelId = result.Keys["channel-id"];
			var channel = channelName != null ? Client.FindChannel(channelName) : Client.GetChannel(channelId);
			var file = result.Keys["file-name"];


			var cutoff = DateTime.MaxValue;
			// TODO: we probably want to drop any messages before the cutoff point to prevent duplicates
			command.Reply("this might take a while...");

			List<ChatMessage> sorted;
			if (file == null)
			{
				sorted = ImportFromBacklog(channel, cutoff);
			}
			else
			{
				sorted = ImportFromFile(channel, file);
			}

			var inserted = StatsDatabase.Import(sorted);
			if (sorted.Count == inserted)
			{
				command.Reply("done!");
			}
			else
			{
				command.Reply($"something went wrong. {inserted} out of {sorted.Count} messages have been inserted.");
			}
		}

		private List<ChatMessage> ImportFromFile(ChatChannel channel, string file)
		{
			throw new NotImplementedException();
		}

		private List<ChatMessage> ImportFromBacklog(ChatChannel channel, DateTime cutoff)
		{
			var messages = Client.GetBacklog(channel, cutoff, DateTime.MinValue).Select(m =>
			{
				m.Sender.BindDbUser(StatsDatabase.UpsertUser(m.Sender));
				return m;
			}).ToList();

			var sorted = messages.OrderBy(m => m.SentAt).ToList();
			return sorted;
		}
	}
}
