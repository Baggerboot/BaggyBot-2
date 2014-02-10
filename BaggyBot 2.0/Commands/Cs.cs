using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;

using Mono.CSharp;

namespace BaggyBot.Commands
{
	class Cs : ReadEvaluatePrintCommand, ICommand
	{
		private Evaluator evaluator;
		private CodeFormatter codeFormatter = new CodeFormatter();
		private IrcReportPrinter reportPrinter = new IrcReportPrinter();
		private Dictionary<string, StringBuilder> commandBuilders = new Dictionary<string, StringBuilder>();

		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Cs(IrcInterface inter)
		{
			ircInterface = inter;
			evaluator = new Evaluator(new CompilerContext(new CompilerSettings() { Unsafe = true }, reportPrinter));
			evaluator.DescribeTypeExpressions = true;
			evaluator.WaitOnTask = true;
			bool resultSet;
			object result;
			string res = evaluator.Evaluate("using System; using System.Linq; using System.Collections.Generic; using System.Collections; using System.Text;", out result, out resultSet);
		}

		protected override void Abort(CommandArgs command)
		{
			evaluator.Interrupt();
		}

		protected override void GetBuffer(CommandArgs command)
		{
			ircInterface.SendMessage(command.Channel, string.Format("{0}, \"{1}\"", command.Sender.Nick, commandBuilders[command.Sender.Nick].ToString().Replace('\n', '\\')));
		}

		protected override void Threads(CommandArgs command)
		{
			throw new NotImplementedException();
		}

		private bool RestrictionsCheck(CommandArgs command)
		{
			if (!command.Channel.StartsWith("#")) {
				ircInterface.SendMessage(command.Channel, "Only the bot operator is allowed to execute Python code in non-channels");
				return false;
			}
			if (security == InterpreterSecurity.Block) {
				ircInterface.SendMessage(command.Channel, "For security reasons, the interactive Python interpreter is currently blocked. Please try again later or ask baggerboot to unblock it.");
				return false;
			}
			if (security == InterpreterSecurity.Notify) {
				// Do not return anything yet, but do notify the bot operator.
				ircInterface.NotifyOperator("-cs used by " + command.Sender.Nick + ": " + command.FullArgument);
			}
			if (command.FullArgument != null && (command.FullArgument.ToLower().Contains("ircinterface") || command.FullArgument.ToLower().Contains("datafunctionset"))) {
				ircInterface.SendMessage(command.Channel, "Access to my guts is restricted to the operator.");
				return false;
			} if (command.FullArgument != null && (command.FullArgument.Contains("Process"))) {
				ircInterface.SendMessage(command.Channel, "Process control is restricted to the operator.");
				return false;
			} if (command.FullArgument != null && (command.FullArgument.Contains("GetMethod"))) {
				ircInterface.SendMessage(command.Channel, "Method invocation trough reflection is restricted to the operator.");
				return false;
			} if (command.FullArgument != null && (command.FullArgument.Contains("Environment.Exit"))) {
				ircInterface.SendMessage(command.Channel, "Calls to Environment.Exit are not allowed");
				return false;
			}
			return true;
		}

		public void Use(CommandArgs command)
		{
			bool isOperator = Tools.UserTools.Validate(command.Sender);

			if (!commandBuilders.ContainsKey(command.Sender.Nick)) {
				commandBuilders.Add(command.Sender.Nick, new StringBuilder());
			}

			if (command.FullArgument == null) {
				command.Reply("Usage: -cs <C# code>");
				return;
			}

			if (command.FullArgument.Contains("Console.Write")) {
				ircInterface.SendMessage(command.Channel, "Console.Write calls are not supported yet.");
				return;
			}
			if (!(isOperator || RestrictionsCheck(command))) {
				return;
			}
			if (command.FullArgument != null) {
				if (command.FullArgument.StartsWith("--")) {
					command.FullArgument = command.FullArgument.Substring(2);
					ProcessControlCommand(command);
					return;
				}
			}

			bool resultSet;
			object result;
			try {
				string fullInput = commandBuilders[command.Sender.Nick].ToString() + " " + command.FullArgument;
				fullInput = fullInput.TrimStart();
				string input = evaluator.Evaluate(fullInput, out result, out resultSet);

				if (resultSet) {
					string output = codeFormatter.PrettyPrint(result);
					ircInterface.SendMessage(command.Channel, "--> " + output);
					commandBuilders[command.Sender.Nick].Clear();
				} else if (input == null) {
					if (reportPrinter.HasMessage) {
						var message = reportPrinter.GetNextMessage();
						ircInterface.SendMessage(command.Channel, string.Format("{0} at column {1}: {2}", message.MessageType, message.Location.Column, message.Text));
					} else {
						ircInterface.SendMessage(command.Channel, "Done (No result)");
					}
					commandBuilders[command.Sender.Nick].Clear();
					return;
				} else {
					commandBuilders[command.Sender.Nick].Append(input);
					ircInterface.SendMessage(command.Channel, ">>>");
				}

			} catch (InternalErrorException e) {
				ircInterface.SendMessage(command.Channel, "Exception: " + e.ToString());
				return;
			}
		}
	}
}
