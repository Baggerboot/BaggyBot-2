using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using BaggyBot.Commands;
using BaggyBot.Database;

namespace BaggyBot.DataProcessors
{
	class CommandHandler
	{
		// Maps command strings to command objects
		private Dictionary<string, ICommand> commands;
		private IrcInterface ircInterface;
		private BotDiagnostics botDiagnostics;

		public CommandHandler(IrcInterface ircInterface, DataFunctionSet dataFunctionSet, Bot program, BotDiagnostics botDiagnostics)
		{
			this.ircInterface = ircInterface;
			this.botDiagnostics = botDiagnostics;
			commands = new Dictionary<string, ICommand>()
			{
				{"convert", new BaggyBot.Commands.Convert()},
				{"crash", new Crash(dataFunctionSet)},
				{"cs", new Cs(ircInterface)},
				{"feature", new Feature(dataFunctionSet)},
				{"get", new Get(dataFunctionSet)},
				{"html", new Html()},
				{"help", new Help()},
				{"join", new Join(ircInterface, dataFunctionSet)},
				{"ns", new NickServ(dataFunctionSet, ircInterface)},
				{"part", new Part(ircInterface)},
				{"ping", new Ping()},
				{"py", new Py(ircInterface, dataFunctionSet)},
				{"rdns", new ResolveReverse()},
				{"regen", new RegenerateGraphs()},
				{"rem", new Remember()},
				{"resolve", new Resolve()},
				{"say", new Say(ircInterface)},
				{"stats", new Stats()},
				{"set", new Set(dataFunctionSet)},
				{"shutdown", new Shutdown(program)},
				{"snag", new Snag()},
				{"uptime", new Uptime()},
				{"version", new Commands.Version()}
			};
		}

		public void ProcessCommand(IRCSharp.IrcMessage message)
		{
			Logger.Log("Processing command: " + message.Message);
			if(message.Message.Equals(Bot.commandIdentifier)) return;

			string line = message.Message.Substring(1);
			if (new string[] { "help", "about", "info", "baggybot", "stats" }.Contains(message.Message.ToLower().Substring(1))) {
				ircInterface.SendMessage(message.Channel, "BaggyBot " + Bot.Version + " -- Stats page: http://www.jgeluk.net/stats -- Made by baggerboot. For help, try the -help command.");
			}
			string[] args = line.Split(' ');
			string command = args[0];
			args = args.Skip(1).ToArray();

			int cmdIndex = line.IndexOf(' ');
			CommandArgs cmd = new CommandArgs(command, args, message.Sender, message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex+1), ircInterface.SendMessage);

			if (!commands.ContainsKey(command)) {
				((Remember)commands["rem"]).UseRem(cmd);
				return;
			}

			if (commands[command].Permissions == PermissionLevel.All || commands[command].Permissions == PermissionLevel.BotOperator && Tools.UserTools.Validate(message.Sender)) {
				if (Settings.Instance["deployed"] == "true") {
					try {
						commands[command].Use(cmd);
					} catch (Exception e) {
						string exceptionMessage = string.Format("An unhandled exception (type: {0}) occurred while trying to process your command! Exception message: \"{1}\", Occurred at line {2}", e.GetType().ToString(), e.Message, new StackTrace(e, true).GetFrame(0).GetFileLineNumber());
						ircInterface.SendMessage(message.Channel, exceptionMessage);
					}
				} else {
					commands[command].Use(cmd);
				}
			} else {
				ircInterface.SendMessage(message.Channel, Messages.CMD_NOT_AUTHORIZED);
			}
		}
	}
}
