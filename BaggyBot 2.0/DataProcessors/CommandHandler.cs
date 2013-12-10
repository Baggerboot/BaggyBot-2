using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using BaggyBot.Commands;
using BaggyBot.Database;

namespace BaggyBot
{
	class CommandHandler
	{
		private Dictionary<string, ICommand> commands;
		private IrcInterface ircInterface;
		private BotDiagnostics botDiagnostics;

		public CommandHandler(IrcInterface ircInterface, DataFunctionSet dataFunctionSet, Bot program, BotDiagnostics botDiagnostics)
		{
			this.ircInterface = ircInterface;
			this.botDiagnostics = botDiagnostics;
			commands = new Dictionary<string, ICommand>()
			{
				{"crash", new Crash(ircInterface, dataFunctionSet)},
				{"elycool", new Elycool(ircInterface)},
				{"ed", new ExceptionDetails(ircInterface, botDiagnostics)},
				{"feature", new Feature(ircInterface, dataFunctionSet)},
				{"get", new Get(ircInterface, dataFunctionSet)},
				{"html", new Html(ircInterface)},
				{"help", new Help(ircInterface)},
				{"join", new Join(ircInterface, dataFunctionSet)},
				{"ns", new NickServ(ircInterface, dataFunctionSet)},
				{"part", new Part(ircInterface)},
				{"ping", new Ping(ircInterface)},
				{"py", new Py(ircInterface, dataFunctionSet)},
				{"rdns", new ResolveReverse(ircInterface)},
				{"regen", new RegenerateGraphs(ircInterface)},
				{"rem", new Remember(ircInterface)},
				{"resolve", new Resolve(ircInterface)},
				{"say", new Say(ircInterface)},
				{"set", new Set(ircInterface, dataFunctionSet)},
				{"shutdown", new Shutdown(ircInterface, program)},
				{"snag", new Snag(ircInterface)},
				{"version", new Commands.Version(ircInterface)}
			};
		}

		public void ProcessCommand(IRCSharp.IrcMessage message)
		{
			Logger.Log("Processing command: " + message.Message);
			if(message.Message.Equals(Bot.commandIdentifier)) return;

			string line = message.Message.Substring(1);
			if (new string[] { "help", "about", "info", "baggybot", "stats" }.Contains(message.Message.ToLower())) {
				ircInterface.SendMessage(message.Channel, "BaggyBot " + Bot.Version + " -- Stats page: http://www.jgeluk.net/stats -- Made by baggerboot. For help, try the -help command.");
			}
			string[] args = line.Split(' ');
			string command = args[0];
			args = args.Skip(1).ToArray();

			int cmdIndex = line.IndexOf(' ');
			CommandArgs cmd = new CommandArgs(command, args, message.Sender, message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex+1));

			if (!commands.ContainsKey(command)) {
				((Remember)commands["rem"]).UseRem(cmd);
				return;
			}

			if (commands[command].Permissions == PermissionLevel.All || commands[command].Permissions == PermissionLevel.BotOperator && Tools.UserTools.Validate(message.Sender)) {
				try {
					commands[command].Use(cmd);
				} catch (Exception e) {
					string exceptionMessage = string.Format("An unhandled exception (type: {0}) occurred while trying to process your command! Exception message: \"{1}\", Occurred at line {2}", e.GetType().ToString(), e.Message, new StackTrace(e, true).GetFrame(0).GetFileLineNumber());
					ircInterface.SendMessage(message.Channel, exceptionMessage);
					botDiagnostics.Exceptions.Add(e);
				}
			} else {
				ircInterface.SendMessage(message.Channel, Messages.CMD_NOT_AUTHORIZED);
			}
		}
	}
}
