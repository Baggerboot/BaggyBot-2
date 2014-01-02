using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	abstract class ReadEvaluatePrintCommand
	{
		protected int threadId = 0;
		protected IrcInterface ircInterface;
		protected InterpreterSecurity security = InterpreterSecurity.Block;
		protected enum InterpreterSecurity
		{
			Allow,
			Notify,
			Block,
		}

		private void SetSecurity(string setting)
		{
			security = (InterpreterSecurity)Enum.Parse(typeof(InterpreterSecurity), setting, true);
		}
		protected abstract void Abort(CommandArgs command);
		protected abstract void Threads(CommandArgs command);
		protected abstract void GetBuffer(CommandArgs command);
		protected void ProcessControlCommand(CommandArgs command)
		{
			if (!Tools.UserTools.Validate(command.Sender)) {
				ircInterface.SendMessage(command.Channel, "Python Interpreter control commands may only be used by the bot operator");
				return;
			}
			string control = command.Args[0].Substring(2);
			switch (control) {
				case "security":
					try {
						SetSecurity(command.Args[1]);
					} catch (ArgumentException) {
						ircInterface.SendMessage(command.Channel, command.Args[1] + ": Invalid security level");
						break;
					}
					ircInterface.SendMessage(command.Channel, "Security level set to " + security.ToString());
					break;
				case "abort":
					Abort(command);
					break;
				case "toggle":
					ControlVariables.QueryConsole = !ControlVariables.QueryConsole;
					ircInterface.SendMessage(command.Channel, "Interactive query console: " + (ControlVariables.QueryConsole ? "On" : "Off"));
					break;
				case "help":
					ircInterface.SendMessage(command.Channel, "The following control commands are available: security, abort, toggle");
					break;
				case "threads":
					Threads(command);
					break;
				case "buffer":
					GetBuffer(command);
					break;
				default:
					ircInterface.SendMessage(command.Channel, "That is not a valid control command.");
					break;
			}
		}
	}
}
