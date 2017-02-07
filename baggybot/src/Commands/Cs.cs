using System;
using System.Collections.Generic;
using System.Text;
using BaggyBot.Commands.Interpreters;
using BaggyBot.Commands.Interpreters.CSharp;
using BaggyBot.Tools;
using Mono.CSharp;

namespace BaggyBot.Commands
{
	internal class Cs : ReadEvaluatePrintCommand
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "cs";
		public override string Usage => "<C# code>";
		public override string Description => "Executes the given C# code and prints its result to IRC.";

		private readonly Evaluator evaluator;
		private readonly IrcReportPrinter reportPrinter = new IrcReportPrinter();
		private readonly Dictionary<string, StringBuilder> commandBuilders = new Dictionary<string, StringBuilder>();
		
		public Cs()
		{
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
			command.ReturnMessage($"{command.Sender.Nickname}, \"{commandBuilders[command.Sender.Nickname].ToString().Replace('\n', '\\')}\"");
		}

		protected override void Threads(CommandArgs command)
		{
			throw new NotImplementedException("Cs.Threads");
		}

		private bool RestrictionsCheck(CommandArgs command)
		{
			if (command.Channel.IsPrivateMessage)
			{
				command.ReturnMessage("Only the bot operator is allowed to execute Python code in non-channels");
				return false;
			}
			if (Security == InterpreterSecurity.Block)
			{
				command.ReturnMessage("For security reasons, the interactive C# interpreter is currently blocked. Please try again later or ask Baggykiin to unblock it.");
				return false;
			}
			if (Security == InterpreterSecurity.Notify)
			{
				// Do not return anything yet, but do notify the bot operator.
				Client.NotifyOperators("-cs used by " + command.Sender.Nickname + ": " + command.FullArgument);
			}
			if (command.FullArgument != null && (command.FullArgument.ToLower().Contains("ircinterface") || command.FullArgument.ToLower().Contains("datafunctionset")))
			{
				command.ReturnMessage("Access to my guts is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("Process"))
			{
				command.ReturnMessage("Process control is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("GetMethod"))
			{
				command.ReturnMessage("Method invocation trough reflection is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("Environment.Exit"))
			{
				command.ReturnMessage("Calls to Environment.Exit are not allowed");
				return false;
			}
			return true;
		}

		public override void Use(CommandArgs command)
		{
			var isOperator = Client.Validate(command.Sender);

			if (!commandBuilders.ContainsKey(command.Sender.Nickname))
			{
				commandBuilders.Add(command.Sender.Nickname, new StringBuilder());
			}

			if (command.FullArgument == null)
			{
				command.Reply("Usage: -cs <C# code>");
				return;
			}

			if (command.FullArgument.Contains("Console.Write"))
			{
				command.ReturnMessage("Console.Write calls are not supported yet.");
				return;
			}
			if (!(isOperator || RestrictionsCheck(command)))
			{
				return;
			}
			if (command.FullArgument.StartsWith("--"))
			{
				ProcessControlCommand(CommandArgs.FromPrevious(command.Command, command.FullArgument.Substring(2), command));
				return;
			}

			try
			{
				var fullInput = commandBuilders[command.Sender.Nickname] + " " + command.FullArgument;
				fullInput = fullInput.TrimStart();
				bool resultSet;
				object result;
				var input = evaluator.Evaluate(fullInput, out result, out resultSet);

				if (resultSet)
				{
					var output = CodeFormatter.PrettyPrint(result);
					command.ReturnMessage("--> " + output);
					commandBuilders[command.Sender.Nickname].Clear();
				}
				else if (input == null)
				{
					if (reportPrinter.HasMessage)
					{
						while (reportPrinter.HasMessage)
						{
							var message = reportPrinter.GetNextMessage();
							command.ReturnMessage($"{message.MessageType} at column {message.Location.Column}: {message.Text}");
						}
					}
					else
					{
						command.ReturnMessage("Done (No result)");
					}
					commandBuilders[command.Sender.Nickname].Clear();
				}
				else
				{
					commandBuilders[command.Sender.Nickname].Append(input);
					command.ReturnMessage(">>>");
				}
			}
			catch (InternalErrorException e)
			{
				command.ReturnMessage("Exception: " + e);
			}
		}
	}
}
