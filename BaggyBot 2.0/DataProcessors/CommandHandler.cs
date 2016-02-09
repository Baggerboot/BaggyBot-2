using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BaggyBot.Commands;
using BaggyBot.Configuration;
using BaggyBot.DataProcessors.IO;
using BaggyBot.Tools;
using IRCSharp.IRC;
using Convert = BaggyBot.Commands.Convert;
using Roslyn = BaggyBot.Commands.RoslynExec;
using Version = BaggyBot.Commands.Version;
using WolframAlpha = BaggyBot.Commands.WolframAlpha;

namespace BaggyBot.DataProcessors
{
	internal class CommandHandler
	{
		private readonly Dictionary<string, Command> commands;
		private readonly IrcInterface ircInterface;

		public CommandHandler(IrcInterface ircInterface, DataFunctionSet dataFunctionSet, Bot bot)
		{
			this.ircInterface = ircInterface;
			commands = new Dictionary<string, Command>()
			{
				{"alias", new Alias(dataFunctionSet)},
				{"bf", new Bf()},
				{"convert", new Convert()},
				{"feature", new Feature(dataFunctionSet)},
				{"get", new Get(dataFunctionSet, ircInterface)},
				{"html", new Html()},
				{"http", new HttpInterface()},
				{"join", new Join(ircInterface)},
				{"ns", new NickServ(dataFunctionSet, ircInterface)},
				{"part", new Part(ircInterface)},
				{"ping", new Ping()},
				{"reconnect", new Reconnect(ircInterface)},
				{"rdns", new ResolveReverse()},
				{"regen", new RegenerateGraphs()},
				{"resolve", new Resolve()},
				{"roslyn", new RoslynExec()},
				{"say", new Say()},
				{"set", new Set(dataFunctionSet)},
				{"shutdown", new Shutdown(bot)},
				{"snag", new Snag()},
				{"ur", new UrbanDictionary()},
				{"update", new Update(bot)},
				{"uptime", new Uptime()},
				{"version", new Version()},
				{"wa", new WolframAlpha()},
				{"wiki", new Wikipedia()},
				{"topics", new Topics(dataFunctionSet)}
			};
			// Command list must be initialised before we can pass a reference to it to the Help command.
			commands.Add("help", new Help(commands));

			if (ConfigManager.Config.Interpreters.Enabled)
			{
				commands.Add("py", new Py(ircInterface, dataFunctionSet));
				commands.Add("cs", new Cs(ircInterface));
			}
			else
			{
				commands.Add("py", new Notify("The interactive Python interpreter is currently disabled. It can be enabled in the configuration file."));
				commands.Add("cs", new Notify("The interactive C# interpreter is currently disabled. It can be enabled in the configuration file."));
			}
		}

		private CommandArgs BuildCommand(IrcMessage message)
		{
			var line = message.Message.Substring(1);

			var args = line.Split(' ');
			var command = args[0];
			args = args.Skip(1).ToArray();

			var cmdIndex = line.IndexOf(' ');
			return new CommandArgs(command, args, message.Sender, message.Channel, cmdIndex == -1 ? null : line.Substring(cmdIndex + 1), ircInterface.SendMessage);
		}

		public void ProcessCommand(IrcMessage message)
		{
			Logger.Log(this, "Processing command: " + message.Message);
			if (message.Message.Equals(Bot.CommandIdentifier)) return;

			var cmdInfo = BuildCommand(message);

			// Inject bot information, but do not return.
			if (new[] { "help", "about", "info", "baggybot", "stats" }.Contains(message.Message.ToLower().Substring(1)))
			{
				ircInterface.SendMessage(message.Channel, string.Format(Messages.CmdGeneralInfo, Bot.Version));
			}

			if (!commands.ContainsKey(cmdInfo.Command))
			{
				if (cmdInfo.Command == "rem")
				{
					Logger.Log(this, "Saving rem");
					var value = cmdInfo.Args.ToList();
					value.Insert(1, "say");
					((Alias)commands["alias"]).Use(
						new CommandArgs("alias", value.ToArray(), cmdInfo.Sender, cmdInfo.Channel, string.Join(" ", value), cmdInfo.ReplyCallback));
				}
				else if (((Alias)commands["alias"]).ContainsKey(cmdInfo.Command))
				{
					var aliasedCommand = ((Alias)commands["alias"]).GetAlias(cmdInfo.Command);
					if(cmdInfo.FullArgument == null)
					{
						aliasedCommand = aliasedCommand.Replace(" $args", "");
					}
					else
					{
						aliasedCommand = aliasedCommand.Replace("$args", cmdInfo.FullArgument);
					}
					Logger.Log(this, $"Calling aliased command: -{aliasedCommand}");
					ProcessCommand(new IrcMessage(message.Sender, message.Channel, "-" + aliasedCommand, message.Action));
				}
				return;
			}

			if (commands[cmdInfo.Command].Permissions == PermissionLevel.All || commands[cmdInfo.Command].Permissions == PermissionLevel.BotOperator && UserTools.Validate(message.Sender))
			{
				// Don't gobble up exceptions when debugging
				if (ConfigManager.Config.DebugMode)
				{
					commands[cmdInfo.Command].Use(cmdInfo);
				}
				else
				{
					try
					{
						commands[cmdInfo.Command].Use(cmdInfo);
					}
					catch (Exception e)
					{
						var exceptionMessage = $"An unhandled exception (type: {e.GetType()}) occurred while trying to process your command! Exception message: \"{e.Message}\", Occurred at line {new StackTrace(e, true).GetFrame(0).GetFileLineNumber()}";
						ircInterface.SendMessage(message.Channel, exceptionMessage);
					}
				}
			}
			else
			{
				ircInterface.SendMessage(message.Channel, Messages.CmdNotAuthorized);
			}
		}
	}
}
