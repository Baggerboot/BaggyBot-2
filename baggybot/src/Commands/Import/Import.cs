using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaggyBot.CommandParsing;
using BaggyBot.MessagingInterface;
using BaggyBot.Tools;
using LinqToDB.Expressions;
using Mono.CSharp;
using Newtonsoft.Json;

namespace BaggyBot.Commands.Import
{
	class Import : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "import";
		public override string Usage => "[<-n|--channel-name> channel][<-c|--channel-id> channel-id][-m|--map-users][<-f|--file-name> file][<-t|--file-type> <slack-history>][<-b|--into-buffer>]";
		public override string Description => "Imports messages for the given channel, or the current channel if no channel is specified.";

		private List<ChatMessage> buffer = new List<ChatMessage>();

		public override void Use(CommandArgs command)
		{
			// TODO: implement map-users
			var parser = new CommandParser(
				new Operation()
					.AddKey("channel-name", null, 'c')
					.AddKey("channel-id", command.Channel.Identifier, 'C')
					.AddKey("file-name", null, 'f')
					.AddKey("file-type", "slack-history", 't')
					.AddFlag("into-buffer", 'b'))
				.AddOperation("transform", new Operation()
					.AddKey("channel-name", null, 'c')
					.AddKey("channel-id", command.Channel.Identifier, 'C'))
				.AddOperation("commit", new Operation());

			var result = parser.Parse(command.FullArgument);

			if (result.OperationName == "transform")
			{
				Transform(command, result);
			}
			else if (result.OperationName == "commit")
			{
				var inserted = StatsDatabase.Import(buffer);

				if (buffer.Count == inserted) command.Reply($"{inserted} messages have been added to the database. Oldest: {buffer.First().SentAt} - Newest: {buffer.Last().SentAt}");
				else command.Reply($"something went wrong. {inserted} out of {buffer.Count} messages have been inserted.");
			}
			else
			{
				ImportMessages(command, result);
			}
		}

		private void Transform(CommandArgs command, OperationResult result)
		{
			var channelName = result.Keys["channel-name"];
			var channelId = result.Keys["channel-id"];
			var channel = channelName != null ? Client.FindChannel(channelName) : Client.GetChannel(channelId);

			buffer = buffer.Select(m => new ChatMessage(m.SentAt, m.Sender, channel, m.Body, m.Action)).ToList();

			command.Reply($"All messages in the buffer have been moved to {channel}");
		}

		private void ImportMessages(CommandArgs command, OperationResult result)
		{
			var channelName = result.Keys["channel-name"];
			var channelId = result.Keys["channel-id"];
			var channel = channelName != null ? Client.FindChannel(channelName) : Client.GetChannel(channelId);
			var file = result.Keys["file-name"];
			var filetype = result.Keys["file-type"];
			var intoBuffer = result.Flags["into-buffer"];

			var cutoff = DateTime.Now;
			// TODO: we probably want to drop any messages before the cutoff point to prevent duplicates
			command.Reply("this might take a while...");

			List<ChatMessage> sorted;
			try
			{
				if (file == null)
				{
					sorted = ImportFromBacklog(channel, cutoff);
				}
				else
				{
					sorted = ImportFromFile(channel, file, filetype);
				}
			}
			catch (Exception e)
			{
				command.Reply($"Failed to import messages. An exception occurred: ({e.Format()})");
				return;
			}
			if (intoBuffer)
			{
				buffer = buffer.Concat(sorted).OrderBy(m => m.SentAt).ToList();
				command.Reply($"{sorted.Count} messages have been added to the buffer (total: {buffer.Count} messages). Oldest: {sorted.First().SentAt} - Newest: {sorted.Last().SentAt} -- Oldest in buffer: {buffer.First().SentAt} - Newest: {buffer.Last().SentAt}");
			}
			else
			{
				var inserted = StatsDatabase.Import(sorted);

				if (sorted.Count == inserted) command.Reply($"{inserted} messages have been added to the database. Oldest: {sorted.First().SentAt} - Newest: {sorted.Last().SentAt}");
				else command.Reply($"something went wrong. {inserted} out of {sorted.Count} messages have been inserted.");
			}
		}

		private List<ChatMessage> ImportFromFile(ChatChannel channel, string file, string filetype)
		{
			var contents = File.ReadAllText(file);
			switch (filetype)
			{
				case "slack-history":
					return ImportFromSlackHistory(channel, contents);
				default:
					throw new ArgumentException("Invalid file type: " + filetype);
			}
		}

		private List<ChatMessage> ImportFromSlackHistory(ChatChannel channel, string contents)
		{
			var data = JsonConvert.DeserializeObject<SlackHistory.ChannelImport>(contents);

			var messages = data.messages.Where(m => m.subtype != "file_comment" && m.subtype != "bot_message").Select(m => new ChatMessage(m.ts,
																	 Client.GetUser(m.user),
																	 new ChatChannel(data.channel_info.id, data.channel_info.name),
																	 m.text)).OrderBy(m => m.SentAt).ToList();
			return messages;
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
