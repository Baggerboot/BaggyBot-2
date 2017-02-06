using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Commands;
using BaggyBot.Configuration;
using BaggyBot.EmbeddedData;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;
using BaggyBot.Tools;
using Convert = BaggyBot.Commands.Convert;
using Version = BaggyBot.Commands.Version;
using WolframAlpha = BaggyBot.Commands.WolframAlpha;

namespace BaggyBot.DataProcessors
{
	internal class CommandHandler
	{
		private readonly Dictionary<string, Command> commands;

		public CommandHandler(Bot bot)
		{
			commands = new Dictionary<string, Command>()
			{
				{"alias", new Alias()},
				{"bf", new Bf()},
				{"convert", new Convert()},
				{"feature", new Feature()},
				{ "g", new GoogleSearch()},
				{"get", new Get()},
				{"http", new HttpInterface()},
				{"join", new Join()},
				{"ns", new NickServ()},
				{"part", new Part()},
				{"ping", new Ping()},
				{"reconnect", new Reconnect()},
				{"rdns", new ResolveReverse()},
				{"regen", new RegenerateGraphs()},
				{"reset", new ResetDb()},
				{"resolve", new Resolve()},
				{"say", new Say()},
				{"search", new Search()},
				{"set", new Set()},
				{"shutdown", new Shutdown(bot)},
				{"snag", new Snag()},
				{"sql", new Sql() },
				{"ur", new UrbanDictionary()},
				{"update", new Update()},
				{"uptime", new Uptime()},
				{"version", new Version()},
				{"wa", new WolframAlpha()},
				{"wiki", new Wikipedia()},
				{"whoami", new WhoAmI()},
				{"topics", new Topics()}
			};
			// Command list must be initialised before we can pass a reference to it to the Help command.
			commands.Add("help", new Help(commands));

			if (ConfigManager.Config.Interpreters.Enabled)
			{
				commands.Add("py", new Py());
				commands.Add("cs", new Cs());
				commands.Add("roslyn", new RoslynExec());
			}
			else
			{
				commands.Add("py", new Notify("The interactive Python interpreter is currently disabled. It can be enabled in the configuration file."));
				commands.Add("cs", new Notify("The interactive C# interpreter is currently disabled. It can be enabled in the configuration file."));
			}
		}

		public void ProcessMessage(ChatMessage message)
		{
			Logger.Log(this, "Processing command: " + message.Message);

			if (Bot.CommandIdentifiers.Any(id => id == message.Message)) return;

			var cmdInfo = CommandArgs.FromMessage(message);
			ProcessCommand(cmdInfo);
		}

		private void ProcessCommand(CommandArgs cmdInfo)
		{
			// Inject bot information, but do not return.
			if (new[] { "help", "about", "info", "baggybot", "stats" }.Contains(cmdInfo.Command.ToLower()) && cmdInfo.Args.Length == 0)
			{
				cmdInfo.ReturnMessage(string.Format(Messages.CmdGeneralInfo, Bot.Version, ConfigManager.Config.StatsPage));
			}

			if (!commands.ContainsKey(cmdInfo.Command))
			{
				if (cmdInfo.Command == "rem")
				{
					Logger.Log(this, "Saving rem");
					var value = cmdInfo.Args.ToList();
					value.Insert(1, "say");
					((Alias)commands["alias"]).Use(
						new CommandArgs(cmdInfo.Client, "alias", value.ToArray(), cmdInfo.Sender, cmdInfo.Channel, string.Join(" ", value)));
				}
				else if (((Alias)commands["alias"]).ContainsKey(cmdInfo.Client.StatsDatabase, cmdInfo.Command))
				{
					var aliasedCommand = ((Alias)commands["alias"]).GetAlias(cmdInfo.Client.StatsDatabase, cmdInfo.Command);
					if (cmdInfo.FullArgument == null)
					{
						aliasedCommand = aliasedCommand.Replace(" $args", "");
					}
					else
					{
						aliasedCommand = aliasedCommand.Replace("$args", cmdInfo.FullArgument);
					}
					Logger.Log(this, $"Calling aliased command: -{aliasedCommand}");

					ProcessCommand(CommandArgs.FromMessage(new ChatMessage(cmdInfo.Client, cmdInfo.Sender, cmdInfo.Channel, "-" + aliasedCommand)));
					//ProcessCommand(new ChatMessage(message.Sender, message.Channel, "-" + aliasedCommand, message.ReplyCallback, message.Action));
				}
				return;
			}

			bool isValid = false;
			if (commands[cmdInfo.Command].Permissions == PermissionLevel.BotOperator)
			{
				try
				{
					isValid = UserTools.Validate(cmdInfo.Sender);

				}
				catch (Exception e)
				{
					cmdInfo.Reply("I am unable to validate your account.");
					return;
				}
			}
			else
			{
				isValid = true;
			}
			if (!isValid)
			{
				cmdInfo.ReturnMessage(Messages.CmdNotAuthorised);
				return;
			}

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
					var exceptionMessage = $"An unhandled exception (type: {e.GetType()}) occurred while trying to process your command! Exception message: \"{e.Message}\"";
					cmdInfo.ReturnMessage(exceptionMessage);
					// Previously, debugging information (filename and line number) were put in the error message.
					// That's dubm, no reason to bother the user with information that's useless to them. Log the exception instead.
					Logger.LogException(commands[cmdInfo.Command], e, $"processing the command \"{cmdInfo.Command} {cmdInfo.FullArgument}\"");
				}
			}
		}
	}
}