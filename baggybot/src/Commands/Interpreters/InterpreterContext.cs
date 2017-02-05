using BaggyBot.Configuration;
using BaggyBot.Database;

namespace BaggyBot.Commands.Interpreters
{
	internal static class InterpreterContext
	{
		public static InterpreterGlobals Globals { get; internal set; }
	}

	public class InterpreterGlobals
	{
		public BotContext Context { get; private set; }

		public InterpreterGlobals(BotContext context)
		{
			Context = context;
		}
	}

	public class BotContext
	{
		public StatsDatabaseManager Db { get; internal set; }
		public Configuration.Configuration Cfg => ConfigManager.Config;
		public Bot Bot { get; internal set; }
	}
}
