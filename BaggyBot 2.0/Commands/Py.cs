using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using System.IO;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Py : ReadEvaluatePrintCommand, ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private ScriptEngine engine;
		private ScriptScope scope;
		private ProducerConsumerStream outputStream;
		private StreamWriter outputStreamWriter;
		private StreamReader outputStreamReader;

		private List<System.Threading.Thread> threads = new List<System.Threading.Thread>();

		private StringBuilder commandBuilder = new StringBuilder();

		public Py(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;

			engine = Python.CreateEngine();
			scope = engine.CreateScope();
			scope.SetVariable("ircInterface", ircInterface);
			scope.SetVariable("dataFunctionSet", df);
			scope.SetVariable("tools", new Tools.PythonTools(ircInterface));
			outputStream = new ProducerConsumerStream();
			outputStreamWriter = new StreamWriter(outputStream);
			outputStreamReader = new StreamReader(outputStream);
			engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);
			//engine.Runtime.IO.SetErrorOutput(outputStream, outputStreamWriter);
		}

		protected override void Abort(CommandArgs command)
		{
			security = InterpreterSecurity.Block;
			commandBuilder.Clear();
			foreach (var thread in threads) {
				thread.Abort();
			}
			ircInterface.SendMessage(command.Channel, string.Format("Done. {0} running threads have been aborted.", threads.Count));
			threads.Clear();
		}
		protected override void Threads(CommandArgs command)
		{
			string result = string.Join(", ", threads.Select(t => t.Name));
			if (result.Length == 0) {
				ircInterface.SendMessage(command.Channel, "Active threads: " + threads);
			} else {
				ircInterface.SendMessage(command.Channel, "No Python threads running right now.");
			}
		}

		protected override void GetBuffer(CommandArgs command)
		{
			ircInterface.SendMessage(command.Channel, string.Format("{0}, \"{1}\"", command.Sender.Nick, commandBuilder.ToString()));
		}

		/// <summary>
		/// This method checks if a regular user (non-operator) is allowed to execute the given command.
		/// </summary>
		/// <param name="command">The command that should be checked.</param>
		/// <returns>True if the user is allowed to execute the command, false if only the bot operator may execute it.</returns>
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
				ircInterface.NotifyOperator("-py used by " + command.Sender.Nick + ": " + command.FullArgument);
			}
			if (command.FullArgument != null && (command.FullArgument.ToLower().Contains("ircinterface") || command.FullArgument.ToLower().Contains("datafunctionset"))) {
				ircInterface.SendMessage(command.Channel, "Access to my guts is restricted to the operator.");
				return false;
			} if (command.FullArgument != null && (command.FullArgument.Contains("System.Diagnostics.Process"))) {
				ircInterface.SendMessage(command.Channel, "Process control is restricted to the operator.");
				return false;
			} if (command.FullArgument != null && (command.FullArgument.Contains("GetMethod"))) {
				ircInterface.SendMessage(command.Channel, "Method invocation trough reflection is restricted to the operator.");
				return false;
			} if (command.FullArgument != null && (command.FullArgument.Contains("import posix"))) {
				ircInterface.SendMessage(command.Channel, "Posix module calls are restricted to the operator.");
				return false;
			} if (command.FullArgument != null && ((command.FullArgument.Contains("putenv") || command.FullArgument.Contains("listdir") || command.FullArgument.Contains("mkdir") || command.FullArgument.Contains("makedirs") || command.FullArgument.Contains("remove") || command.FullArgument.Contains("rename") || command.FullArgument.Contains("rmdir") || command.FullArgument.Contains("exit"))&& command.FullArgument.Contains("os"))) {
				ircInterface.SendMessage(command.Channel, "Posix module calls are restricted to the operator.");
				return false;
			}
			return true;
		}
		public void Use(CommandArgs command)
		{
			threadId++;
			bool isOperator = Tools.UserTools.Validate(command.Sender);

			if (!(isOperator || RestrictionsCheck(command))) {
				return;
			}
			if (command.FullArgument != null) {
				if (command.FullArgument.StartsWith("--")) {
					command.FullArgument = command.FullArgument.Substring(2);
					ProcessControlCommand(command);
					return;
				}
				if (command.FullArgument.EndsWith(":") || command.FullArgument.StartsWith("    ")) {
					commandBuilder.AppendLine(command.FullArgument);
					ircInterface.SendMessage(command.Channel, ">>>");
					return;
				}
			}

			string code;
			if (command.FullArgument == null && commandBuilder.ToString() != string.Empty) {
				code = commandBuilder.ToString();
				commandBuilder.Clear();
			} else if (command.FullArgument != null) {
				code = command.FullArgument;
			} else {
				command.Reply("Usage: -py [python code] - Leave the [python code] parameter out if you want to close the last indented block of a multi-line script.");
				return;
			}
			var source = engine.CreateScriptSourceFromString(code, SourceCodeKind.SingleStatement);
			try {
				System.Threading.Thread.CurrentThread.Name = "PYTHON_THREAD_#" + threadId;
				threads.Add(System.Threading.Thread.CurrentThread);
				source.Execute(scope);

				string line = outputStreamReader.ReadLine();
				if (line == null) {
					ircInterface.SendMessage(command.Channel, "Done (No result)");
				} else {
					for (int i = 0; line != null; i++) {
						if (i > 3 && !isOperator) { // i starts at 0, so when i=4, that would be the 5th line
							ircInterface.SendMessage(command.Channel, "Spam prevention triggered. Sending more than 4 lines is not allowed.");
							outputStreamReader.ReadToEnd();
							break;
						}
						ircInterface.SendMessage(command.Channel, "--> " + line);
						line = outputStreamReader.ReadLine();
						if (line != null && line.Contains("connection_string")) {
							line = outputStreamReader.ReadLine();
						}
						System.Threading.Thread.Sleep(250); // make sure we don't spam the receiving end too much
					}
				}

			} catch (IronPython.Runtime.UnboundNameException e) {
				ircInterface.SendMessage(command.Channel, "Error: " + e.Message);
			} catch (SyntaxErrorException e) {
				ircInterface.SendMessage(command.Channel, "Syntax Error: " + e.Message);
			} catch (IronPython.Runtime.Exceptions.ImportException e) {
				ircInterface.SendMessage(command.Channel, "Import Error: " + e.Message);
			} catch (MissingMemberException e) {
				ircInterface.SendMessage(command.Channel, "Missing member: " + e.Message);
			} catch (DivideByZeroException) {
				ircInterface.SendMessage(command.Channel, "A DivideByZeroException occurred");
			} catch (Exception e) {
				ircInterface.SendMessage(command.Channel, "Unhandled exception: " + e.GetType().ToString() + ": " + e.Message);
			}
			threads.Remove(System.Threading.Thread.CurrentThread);
		}
	}
}
