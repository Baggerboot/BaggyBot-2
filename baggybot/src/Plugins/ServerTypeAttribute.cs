using System;

namespace BaggyBot.Plugins
{
	public class ServerTypeAttribute :Attribute
	{
		public string ServerType { get; }

		public ServerTypeAttribute(string serverType)
		{
			ServerType = serverType;
		}
	}
}