using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BaggyBot.Commands;
using BaggyBot.Tools;
using IRCSharp;
using IRCSharp.IRC;
using Convert = BaggyBot.Commands.Convert;
using Version = BaggyBot.Commands.Version;

namespace BaggyBot.DataProcessors
{
	class CommandHandler
	{
		private readonly Dictionary<string, ICommand> commands;
		private readonly IrcInterface ircInterface;

		public CommandHandler(IrcInterface ircInterface, DataFunctionSet dataFunctionSet, Bot bot)
		{
			this.ircInterface = ircInterface;
			commands = new Dictionary<string, ICommand>()
			{
				{"bf", new Bf()},
				{"convert", new Convert()},
				{"cs", new Cs(ircInterface)},
				{"feature", new Feature(dataFunctionSet)},
				{"get", new Get(dataFunctionSet, ircInterface)},
				{"html", new Html()},
				{"help", new Help()},
				{"join", new Join(ircInterface, dataFunctionSet)},
				{"ns", new NickServ(dataFunctionSet, ircInterface)},
				{"part", new Part(ircInterface)},
				{"ping", new Ping()},
				{"py", new Py(ircInterface, dataFunctionSet)},
				{"reconnect", new Reconnect(ircInterface)},
				{"rdns", new ResolveReverse()},
				{"regen", new RegenerateGraphs()},
				{"rem", new Remember()},
				{"resolve", new Resolve()},
				{"say", new Say(ircInterface)},
				{"set", new Set(dataFunctionSet)},
				{"shutdown", new Shutdown(bot)},
				{"snag", new Snag()},
				{"update", new Update(bot)},
				{"uptime", new Uptime()},
				{"version", new Version()},
				{"wa", new WolframAlpha()},
				{"wiki", new Wikipedia()},
				{"topic", new Topics(dataFunctionSet)}
			};
		}

		public void ProcessCommand(IrcMessage message)
		{
			Logger.Log("Processing command: " + message.Message);
			if (message.Message.Equals(Bot.CommandIdentifier)) return;

            var line = message.Message.Substring(1);

            // Inject bot information, but do not return.
			if (new string[] { "help", "about", "info", "baggybot", "stats" }.Contains(message.Message.ToLower().Substring(1))) {
				ircInterface.SendMessage(message.Channel, string.Format(Messages.CmdGeneralInfo, Bot.Version));
			}

			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			var cmd = new CommandArgs(command, args, message.Sender, message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1), ircInterface.SendMessage);

			if (!commands.ContainsKey(command)) {
				Logger.Log("Dropped command \"{0}\"; I do not recognize this command.", LogLevel.Info, true, message.Message);
                // Try to use it as a rem instead
				((Remember)commands["rem"]).UseRem(cmd);
				return;
			}

			if (commands[command].Permissions == PermissionLevel.All || commands[command].Permissions == PermissionLevel.BotOperator && UserTools.Validate(message.Sender)) {
                // Don't bother with validation when debugging
				if (Settings.Instance["deployed"] == "true") {
					try {
						commands[command].Use(cmd);
					} catch (Exception e) {
						var exceptionMessage = string.Format("An unhandled exception (type: {0}) occurred while trying to process your command! Exception message: \"{1}\", Occurred at line {2}", e.GetType(), e.Message, new StackTrace(e, true).GetFrame(0).GetFileLineNumber());
						ircInterface.SendMessage(message.Channel, exceptionMessage);
					}
				} else {
					commands[command].Use(cmd);
				}
			} else {
				ircInterface.SendMessage(message.Channel, Messages.CmdNotAuthorized);
			}
		}
	}
}
