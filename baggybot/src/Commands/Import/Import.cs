using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaggyBot.CommandParsing;
using BaggyBot.MessagingInterface;
using BaggyBot.Tools;
using Newtonsoft.Json;

namespace BaggyBot.Commands.Import
{
	class Import : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "import";
		public override string Usage => "[load [--channel-name <name> | --channel-id <id> | <--file-name <name> [--file-type <type>]>] | transform [--channel-name <name> | channel-id <id>] | commit]";
		public override string Description => "Imports messages for the given channel, or the current channel if no channel is specified.";

		private List<ChatMessage> buffer = new List<ChatMessage>();

		public override void Use(CommandArgs command)
		{
			var parser = new CommandParser(
				new Operation())
				.AddOperation("load", new Operation()
					.AddKey("channel-name", null, 'n')
					.AddKey("channel-id", null, 'i')
					.AddFlag("current-channel", 'c')
					.AddKey("file-name", null, 'f')
					.AddKey("file-type", "slack-history", 't')
					.AddFlag("from-log", 'l'))
				.AddOperation("transform", new Operation()
					.AddKey("channel-name", null, 'n')
					.AddKey("channel-id", null, 'i')
					.AddFlag("lookup-users")
					.AddFlag("current-channel", 'c')
					.AddFlag("dedup", 'd'))
				.AddOperation("commit", new Operation())
				.AddOperation("dump", new Operation()
					.AddArgument("file-name"));

			var result = parser.Parse(command.FullArgument);

			switch (result.OperationName)
			{
				case "load":
					ImportMessages(command, result);
					break;
				case "transform":
					Transform(command, result);
					break;
				case "commit":
					var inserted = StatsDatabase.Import(buffer);

					if (buffer.Count == inserted) command.Reply($"{inserted} messages have been added to the database. Oldest: {buffer.First().SentAt} - Newest: {buffer.Last().SentAt}");
					else command.Reply($"something went wrong. {inserted} out of {buffer.Count} messages have been inserted.");
					break;
				default:
					command.Reply("unknown operation: " + result.OperationName);
					break;
			}
		}

		private void Transform(CommandArgs command, OperationResult result)
		{
			var channelName = result.Keys["channel-name"];
			var channelId = result.Keys["channel-id"];
			var currentChannel = result.Flags["current-channel"];
			var channel = currentChannel
				? command.Channel
				: (channelName == null
					? (channelId == null ? null : Client.GetChannel(channelId))
					: Client.FindChannel(channelName));

			if (result.Flags["dedup"])
			{
				var deduplicated = buffer.GroupBy(m => m).Select(g => g.First());
				var previous = buffer.Count;
				buffer = deduplicated.ToList();
				var difference = previous - buffer.Count;
				if (difference == 0)
				{
					command.Reply("no duplicate messages were found.");
				}
				else
				{
					command.Reply($"removed {difference} duplicate(s) (prev. buffer size: {previous} - new size: {buffer.Count} messages.");
				}
				return;
			}
			else if (channel != null)
			{
				buffer = buffer.Select(m => new ChatMessage(m.SentAt, m.Sender, channel, m.Body, m.Action)).ToList();
				command.Reply($"all messages in the buffer have been moved to {channel}");
				return;
			}
			command.Reply("please specify a transformation.");
		}

		private void ImportMessages(CommandArgs command, OperationResult result)
		{
			var channelName = result.Keys["channel-name"];
			var channelId = result.Keys["channel-id"];
			var currentChannel = result.Flags["current-channel"];
			var channel = currentChannel
				? command.Channel
				: (channelName == null
					? (channelId == null ? null : Client.GetChannel(channelId))
					: Client.FindChannel(channelName));
			var file = result.Keys["file-name"];
			var filetype = result.Keys["file-type"];
			var fromLog = result.Flags["from-log"];

			var cutoff = DateTime.Now;
			// TODO: we probably want to drop any messages before the cutoff point to prevent duplicates
			command.Reply("this might take a while...");

			List<ChatMessage> sorted;
			try
			{
				if (fromLog)
				{
					sorted = ImportFromLog(channel);
				}
				else if (file == null)
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
			buffer = buffer.Concat(sorted).OrderBy(m => m.SentAt).ToList();
			command.Reply($"{sorted.Count} messages have been added to the buffer (total: {buffer.Count} messages). Oldest: {sorted.First().SentAt} - Newest: {sorted.Last().SentAt} -- Oldest in buffer: {buffer.First().SentAt} - Newest: {buffer.Last().SentAt}");
		}

		private List<ChatMessage> ImportFromLog(ChatChannel channel)
		{
			return StatsDatabase.GetChatLog(channel).Select(l =>
			{
				var dbUser = StatsDatabase.GetUserById(l.SenderId.Value);
				return new ChatMessage(
					l.SentAt,
					new ChatUser(l.Nick, dbUser.UniqueId, preferredName: dbUser.AddressableName),
					new ChatChannel(l.ChannelId, l.Channel), l.Message);
			}).ToList();
		}

		private List<ChatMessage> ImportFromFile(ChatChannel channel, string file, string filetype)
		{
			var contents = File.ReadAllText(file);
			switch (filetype)
			{
				case "slack-history":
					return ImportFromSlackHistory(contents);
				default:
					throw new ArgumentException("Invalid file type: " + filetype);
			}
		}

		private List<ChatMessage> ImportFromSlackHistory(string contents)
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
