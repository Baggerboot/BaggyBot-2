using System;
using System.Collections.Generic;
using System.Text;
using BaggyBot.Tools;
using Mono.CSharp;

namespace BaggyBot.Commands
{
	internal class Cs : ReadEvaluatePrintCommand, ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<C# code>";
		public string Description => "Executes the given C# code and prints its result to IRC.";

		private readonly Evaluator evaluator;
		private readonly CodeFormatter codeFormatter = new CodeFormatter();
		private readonly IrcReportPrinter reportPrinter = new IrcReportPrinter();
		private readonly Dictionary<string, StringBuilder> commandBuilders = new Dictionary<string, StringBuilder>();
		
		public Cs(IrcInterface inter)
		{
			IrcInterface = inter;
			evaluator = new Evaluator(new CompilerContext(new CompilerSettings { Unsafe = true }, reportPrinter))
			{
				DescribeTypeExpressions = true,
				WaitOnTask = true
			};
			bool resultSet;
			object result;
			evaluator.Evaluate("using System; using System.Linq; using System.Collections.Generic; using System.Text;", out result, out resultSet);
		}

		protected override void Abort(CommandArgs command)
		{
			evaluator.Interrupt();
		}

		protected override void GetBuffer(CommandArgs command)
		{
			IrcInterface.SendMessage(command.Channel, $"{command.Sender.Nick}, \"{commandBuilders[command.Sender.Nick].ToString().Replace('\n', '\\')}\"");
		}

		protected override void Threads(CommandArgs command)
		{
			throw new NotImplementedException("Cs.Threads");
		}

		private bool RestrictionsCheck(CommandArgs command)
		{
			if (!command.Channel.StartsWith("#"))
			{
				IrcInterface.SendMessage(command.Channel, "Only the bot operator is allowed to execute Python code in non-channels");
				return false;
			}
			if (Security == InterpreterSecurity.Block)
			{
				IrcInterface.SendMessage(command.Channel, "For security reasons, the interactive C# interpreter is currently blocked. Please try again later or ask baggerboot to unblock it.");
				return false;
			}
			if (Security == InterpreterSecurity.Notify)
			{
				// Do not return anything yet, but do notify the bot operator.
				IrcInterface.NotifyOperator("-cs used by " + command.Sender.Nick + ": " + command.FullArgument);
			}
			if (command.FullArgument != null && (command.FullArgument.ToLower().Contains("ircinterface") || command.FullArgument.ToLower().Contains("datafunctionset")))
			{
				IrcInterface.SendMessage(command.Channel, "Access to my guts is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("Process"))
			{
				IrcInterface.SendMessage(command.Channel, "Process control is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("GetMethod"))
			{
				IrcInterface.SendMessage(command.Channel, "Method invocation trough reflection is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("Environment.Exit"))
			{
				IrcInterface.SendMessage(command.Channel, "Calls to Environment.Exit are not allowed");
				return false;
			}
			return true;
		}

		public void Use(CommandArgs command)
		{
			var isOperator = UserTools.Validate(command.Sender);

			if (!commandBuilders.ContainsKey(command.Sender.Nick))
			{
				commandBuilders.Add(command.Sender.Nick, new StringBuilder());
			}

			if (command.FullArgument == null)
			{
				command.Reply("Usage: -cs <C# code>");
				return;
			}

			if (command.FullArgument.Contains("Console.Write"))
			{
				IrcInterface.SendMessage(command.Channel, "Console.Write calls are not supported yet.");
				return;
			}
			if (!(isOperator || RestrictionsCheck(command)))
			{
				return;
			}
			if (command.FullArgument.StartsWith("--"))
			{
				command.FullArgument = command.FullArgument.Substring(2);
				ProcessControlCommand(command);
				return;
			}

			try
			{
				var fullInput = commandBuilders[command.Sender.Nick] + " " + command.FullArgument;
				fullInput = fullInput.TrimStart();
				bool resultSet;
				object result;
				var input = evaluator.Evaluate(fullInput, out result, out resultSet);

				if (resultSet)
				{
					var output = codeFormatter.PrettyPrint(result);
					IrcInterface.SendMessage(command.Channel, "--> " + output);
					commandBuilders[command.Sender.Nick].Clear();
				}
				else if (input == null)
				{
					if (reportPrinter.HasMessage)
					{
						while (reportPrinter.HasMessage)
						{
							var message = reportPrinter.GetNextMessage();
							IrcInterface.SendMessage(command.Channel, $"{message.MessageType} at column {message.Location.Column}: {message.Text}");
						}
					}
					else
					{
						IrcInterface.SendMessage(command.Channel, "Done (No result)");
					}
					commandBuilders[command.Sender.Nick].Clear();
				}
				else
				{
					commandBuilders[command.Sender.Nick].Append(input);
					IrcInterface.SendMessage(command.Channel, ">>>");
				}
			}
			catch (InternalErrorException e)
			{
				IrcInterface.SendMessage(command.Channel, "Exception: " + e);
			}
		}
	}
}
