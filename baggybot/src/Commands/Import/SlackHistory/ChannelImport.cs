
using System;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BaggyBot.Commands.Import.SlackHistory
{
	public class ChannelImport
	{
		public Message[] messages { get; set; }
		public Channel_Info channel_info { get; set; }
	}

	public class Channel_Info
	{
		public bool is_archived { get; set; }
		public string name { get; set; }
		public string creator { get; set; }
		public Purpose purpose { get; set; }
		public string id { get; set; }
		public bool is_channel { get; set; }
		public Latest latest { get; set; }
		public object[] previous_names { get; set; }
		public string last_read { get; set; }
		public int unread_count { get; set; }
		public bool is_general { get; set; }
		public Topic topic { get; set; }
		[JsonConverter(typeof(SecondEpochConverter))]
		public DateTime created { get; set; }
		public int unread_count_display { get; set; }
		public bool is_member { get; set; }
		public string[] members { get; set; }
	}

	public class Purpose
	{
		[JsonConverter(typeof(SecondEpochConverter))]
		public DateTime last_set { get; set; }
		public string value { get; set; }
		public string creator { get; set; }
	}

	public class Latest
	{
		[JsonConverter(typeof(SecondEpochConverter))]
		public DateTime ts { get; set; }
		public string subtype { get; set; }
		public string type { get; set; }
		public string text { get; set; }
		public Icons icons { get; set; }
		public string bot_id { get; set; }
		public string username { get; set; }
		public object[] attachments { get; set; }
	}

	public class Icons
	{
		public string image_36 { get; set; }
		public string image_48 { get; set; }
		public string image_72 { get; set; }
	}

	public class Topic
	{
		[JsonConverter(typeof(SecondEpochConverter))]
		public DateTime last_set { get; set; }
		public string value { get; set; }
		public string creator { get; set; }
	}

	public class Message
	{
		[JsonConverter(typeof(SecondEpochConverter))]
		public DateTime ts { get; set; }
		public string subtype { get; set; }
		public string type { get; set; }
		public string text { get; set; }
		public Icons1 icons { get; set; }
		public string bot_id { get; set; }
		public string username { get; set; }
		public object[] attachments { get; set; }
		public string user { get; set; }
		public string purpose { get; set; }
	}

	public class Icons1
	{
		public string image_36 { get; set; }
		public string image_48 { get; set; }
		public string image_72 { get; set; }
	}

	public class Field
	{
		public bool _short { get; set; }
		public string title { get; set; }
		public string value { get; set; }
	}

}
