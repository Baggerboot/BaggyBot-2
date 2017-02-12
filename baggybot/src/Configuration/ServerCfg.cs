using System;

namespace BaggyBot.Configuration
{
	public class ServerCfg
	{
		public string ServerName { get; set; } = Guid.NewGuid().ToString();
		public string ServerType { get; set; } = "irc";

		public string Username { get; set; }
		public string Password { get; set; }

		public string Server { get; set; }
		public int Port { get; set; } = 6667;
		public bool UseTls { get; set; } = true;

		public string[] IncludeChannels { get; set; } = new string[0];
		public string[] ExcludeChannels { get; set; } = new string[0];

		public Backend Backend { get; set; } = null;
		public Operator[] Operators { get; set; } = new Operator[0];
		public dynamic PluginSettings { get; set; }
	}
}