using System;

namespace BaggyBot.Configuration
{
	public class ServerCfg
	{
		public string ServerName { get; set; } = Guid.NewGuid().ToString();
		public string Host { get; set; }
		public int Port { get; set; } = 6667;
		public string Password { get; set; } = null;
		public Identity Identity { get; set; } = new Identity();
		public Operator[] Operators { get; set; } = new Operator[0];
		public string[] AutoJoinChannels { get; set; } = new string[0];
		public bool UseTls { get; set; } = false;
		public bool VerifyCertificate { get; set; } = true;
		public string[] CompatModes { get; set; } = new string[0];
	}
}