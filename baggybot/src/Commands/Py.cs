using BaggyBot.DataProcessors;
using BaggyBot.Tools;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BaggyBot.Database;

namespace BaggyBot.Commands
{
	internal class Py : ReadEvaluatePrintCommand, IDisposable
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "<python code>";
		public override string Description => "Executes the given Python code and prints its result to IRC.";

		private readonly ScriptEngine engine;
		private readonly ScriptScope scope;
		private readonly StreamReader outputStreamReader;

		private readonly List<Thread> threads = new List<Thread>();

		private readonly StringBuilder commandBuilder = new StringBuilder();

		public Py(IrcInterface inter, DataFunctionSet df)
		{
			IrcInterface = inter;

			engine = Python.CreateEngine();
			scope = engine.CreateScope();
			scope.SetVariable("ircInterface", IrcInterface);
			scope.SetVariable("dataFunctionSet", df);
			scope.SetVariable("tools", new PythonTools());
			scope.SetVariable("find", new Action<string>(msg => df.FindLine(msg)));
			var outputStream = new ProducerConsumerStream();
			var outputStreamWriter = new StreamWriter(outputStream);
			outputStreamReader = new StreamReader(outputStream);
			engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);
		}

		protected override void Abort(CommandArgs command)
		{
			Security = InterpreterSecurity.Block;
			commandBuilder.Clear();
			foreach (var thread in threads)
			{
				thread.Abort();
			}
			IrcInterface.SendMessage(command.Channel, $"Done. {threads.Count} running threads have been aborted.");
			threads.Clear();
		}
		protected override void Threads(CommandArgs command)
		{
			var result = string.Join(", ", threads.Select(t => t.Name));
			if (result.Length == 0)
			{
				IrcInterface.SendMessage(command.Channel, "Active threads: " + threads);
			}
			else
			{
				IrcInterface.SendMessage(command.Channel, "No Python threads running right now.");
			}
		}

		protected override void GetBuffer(CommandArgs command)
		{
			IrcInterface.SendMessage(command.Channel, $"{command.Sender.Nick}, \"{commandBuilder}\"");
		}

		/// <summary>
		/// This method checks if a regular user (non-operator) is allowed to execute the given command.
		/// </summary>
		/// <param name="command">The command that should be checked.</param>
		/// <returns>True if the user is allowed to execute the command, false if only the bot operator may execute it.</returns>
		private bool RestrictionsCheck(CommandArgs command)
		{
			if (!command.Channel.StartsWith("#"))
			{
				IrcInterface.SendMessage(command.Channel, "Only the bot operator is allowed to execute Python code in non-channels");
				return false;
			}
			if (Security == InterpreterSecurity.Block)
			{
				IrcInterface.SendMessage(command.Channel, "For security reasons, the interactive Python interpreter is currently blocked. Please try again later or ask baggerboot to unblock it.");
				return false;
			}
			if (Security == InterpreterSecurity.Notify)
			{
				// Do not return anything yet, but do notify the bot operator.
				IrcInterface.NotifyOperator("-py used by " + command.Sender.Nick + ": " + command.FullArgument);
			}
			if (command.FullArgument != null && (command.FullArgument.ToLower().Contains("ircinterface") || command.FullArgument.ToLower().Contains("datafunctionset")))
			{
				IrcInterface.SendMessage(command.Channel, "Access to my guts is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("System.Diagnostics.Process"))
			{
				IrcInterface.SendMessage(command.Channel, "Process control is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("GetMethod"))
			{
				IrcInterface.SendMessage(command.Channel, "Method invocation trough reflection is restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && command.FullArgument.Contains("import posix"))
			{
				IrcInterface.SendMessage(command.Channel, "Posix module calls are restricted to the operator.");
				return false;
			}
			if (command.FullArgument != null && ((command.FullArgument.Contains("putenv") || command.FullArgument.Contains("listdir") || command.FullArgument.Contains("mkdir") || command.FullArgument.Contains("makedirs") || command.FullArgument.Contains("remove") || command.FullArgument.Contains("rename") || command.FullArgument.Contains("rmdir") || command.FullArgument.Contains("exit")) && command.FullArgument.Contains("os")))
			{
				IrcInterface.SendMessage(command.Channel, "Posix module calls are restricted to the operator.");
				return false;
			}
			return true;
		}
		public override void Use(CommandArgs command)
		{
			ThreadId++;
			var isOperator = UserTools.Validate(command.Sender);

			if (!(isOperator || RestrictionsCheck(command)))
			{
				return;
			}
			if (command.FullArgument != null)
			{
				if (command.FullArgument.StartsWith("--"))
				{
					command.FullArgument = command.FullArgument.Substring(2);
					ProcessControlCommand(command);
					return;
				}
				if (command.FullArgument.EndsWith(":") || command.FullArgument.StartsWith("    "))
				{
					commandBuilder.AppendLine(command.FullArgument);
					IrcInterface.SendMessage(command.Channel, ">>>");
					return;
				}
				/*if (command.FullArgument == "import antigravity") {
					command.ReturnMessage("--> https://xkcd.com/353/");
					return;
				}*/
			}

			string code;
			if (command.FullArgument == null && commandBuilder.ToString() != string.Empty)
			{
				code = commandBuilder.ToString();
				commandBuilder.Clear();
			}
			else if (command.FullArgument != null)
			{
				code = command.FullArgument;
			}
			else
			{
				command.Reply("Usage: -py [python code] - Leave the [python code] parameter out if you want to close the last indented block of a multi-line script.");
				return;
			}
			var source = engine.CreateScriptSourceFromString(code, SourceCodeKind.SingleStatement);
			threads.Add(Thread.CurrentThread);
			try
			{
				source.Execute(scope);

				var line = outputStreamReader.ReadLine();
				if (line == null)
				{
					IrcInterface.SendMessage(command.Channel, "Done (No result)");
				}
				else
				{
					for (var i = 0; line != null; i++)
					{
						if (i > 3 && !isOperator)
						{ // i starts at 0, so when i=4, that would be the 5th line
							IrcInterface.SendMessage(command.Channel, "Spam prevention triggered. Sending more than 4 lines is not allowed.");
							outputStreamReader.ReadToEnd();
							break;
						}
						IrcInterface.SendMessage(command.Channel, "--> " + line);
						line = outputStreamReader.ReadLine();
						if (line != null && line.Contains("connection_string"))
						{
							line = outputStreamReader.ReadLine();
						}
						Thread.Sleep(250); // make sure we don't spam the receiving end too much
					}
				}
			}
			catch (UnboundNameException e)
			{
				IrcInterface.SendMessage(command.Channel, "Error: " + e.Message);
			}
			catch (SyntaxErrorException e)
			{
				IrcInterface.SendMessage(command.Channel, "Syntax Error: " + e.Message);
			}
			catch (ImportException e)
			{
				IrcInterface.SendMessage(command.Channel, "Import Error: " + e.Message);
			}
			catch (MissingMemberException e)
			{
				IrcInterface.SendMessage(command.Channel, "Missing member: " + e.Message);
			}
			catch (DivideByZeroException)
			{
				IrcInterface.SendMessage(command.Channel, "A DivideByZeroException occurred");
			}
			catch (Exception e)
			{
				IrcInterface.SendMessage(command.Channel, "Unhandled exception: " + e.GetType() + ": " + e.Message);
			}
			threads.Remove(Thread.CurrentThread);
		}

		public void Dispose()
		{
			outputStreamReader.Dispose();
		}
	}
}
