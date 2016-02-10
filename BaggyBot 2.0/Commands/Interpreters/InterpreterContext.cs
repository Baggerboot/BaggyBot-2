using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.DataProcessors;
using BaggyBot.DataProcessors.IO;

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
		public DataFunctionSet Db { get; internal set; }
		public Configuration.Configuration Cfg => ConfigManager.Config;
		public IrcInterface Irc { get; internal set; }
		public Bot Bot { get; internal set; }
	}
}
