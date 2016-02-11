using System;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.DataProcessors;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	public abstract class ReadEvaluatePrintCommand : Command
	{
		protected int ThreadId { get; set; } = 0;
		protected InterpreterSecurity Security { get; set; }
		public enum InterpreterSecurity
		{
			Allow,
			Notify,
			Block,
		}

		protected ReadEvaluatePrintCommand()
		{
			Security = ConfigManager.Config.Interpreters.StartupSecurityLevel;
		}

		private void SetSecurity(string setting)
		{
			Security = (InterpreterSecurity)Enum.Parse(typeof(InterpreterSecurity), setting, true);
		}
		protected abstract void Abort(CommandArgs command);
		protected abstract void Threads(CommandArgs command);
		protected abstract void GetBuffer(CommandArgs command);
		protected void ProcessControlCommand(CommandArgs command)
		{
			if (!UserTools.Validate(command.Sender))
			{
				command.ReturnMessage("Python Interpreter control commands may only be used by the bot operator");
				return;
			}
			var control = command.Args[0].Substring(2);
			switch (control)
			{
				case "security":
					switch (command.Args.Length)
					{
						case 1:
							command.Reply($"the current security level is {Security}");
							break;
						case 2:
							try
							{
								SetSecurity(command.Args[1]);
								command.ReturnMessage("Security level set to " + Security);
							}
							catch (ArgumentException)
							{
								command.ReturnMessage($"Invalid security level: \"{string.Join(" ", command.Args.Skip(1))}\"");
							}
							break;
						default:
							command.ReturnMessage($"Invalid security level: \"{string.Join(" ", command.Args.Skip(1))}\"");
							break;
					}
					break;
				case "abort":
					Abort(command);
					break;
				case "toggle":
					ControlVariables.QueryConsole = !ControlVariables.QueryConsole;
					command.ReturnMessage("Interactive query console: " + (ControlVariables.QueryConsole ? "On" : "Off"));
					break;
				case "help":
					command.ReturnMessage("The following control commands are available: security, abort, toggle");
					break;
				case "threads":
					Threads(command);
					break;
				case "buffer":
					GetBuffer(command);
					break;
				default:
					command.ReturnMessage("That is not a valid control command.");
					break;
			}
		}
	}
}
