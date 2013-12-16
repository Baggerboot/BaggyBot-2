using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using System.IO;

namespace BaggyBot.Commands
{
	class Py : ICommand
	{
		private enum InterpreterSecurity
		{
			Allow,
			Notify,
			Block,
		}
		private class ProducerConsumerStream : Stream
		{
			private readonly MemoryStream innerStream;
			private long readPosition;
			private long writePosition;

			public ProducerConsumerStream()
			{
				innerStream = new MemoryStream();
			}

			public override bool CanRead { get { return true; } }

			public override bool CanSeek { get { return false; } }

			public override bool CanWrite { get { return true; } }

			public override void Flush()
			{
				lock (innerStream) {
					innerStream.Flush();
				}
			}

			public override long Length
			{
				get
				{
					lock (innerStream) {
						return innerStream.Length;
					}
				}
			}

			public override long Position
			{
				get { throw new NotSupportedException(); }
				set { throw new NotSupportedException(); }
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				lock (innerStream) {
					innerStream.Position = readPosition;
					int red = innerStream.Read(buffer, offset, count);
					readPosition = innerStream.Position;

					return red;
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				lock (innerStream) {
					innerStream.Position = writePosition;
					innerStream.Write(buffer, offset, count);
					writePosition = innerStream.Position;
				}
			}
		}

		private InterpreterSecurity security = InterpreterSecurity.Block;
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private ScriptEngine engine;
		private ScriptScope scope;
		private ProducerConsumerStream outputStream;
		private StreamWriter outputStreamWriter;
		private StreamReader outputStreamReader;

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

		private void ProcessControlCommand(CommandArgs command)
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
					security = InterpreterSecurity.Block;
					commandBuilder.Clear();
					break;
				case "toggle":
					ControlVariables.QueryConsole = !ControlVariables.QueryConsole;
					ircInterface.SendMessage(command.Channel, "Interactive query console: " + (ControlVariables.QueryConsole ? "On" : "Off"));
					break;
				case "help":
					ircInterface.SendMessage(command.Channel, "The following control commands are available: security, abort, toggle");
					break;
				default:
					ircInterface.SendMessage(command.Channel, "That is not a valid control command");
					break;
			}
		}

		private void SetSecurity(string setting)
		{
			security = (InterpreterSecurity)Enum.Parse(typeof(InterpreterSecurity), setting, true);
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
				ircInterface.SendMessage(Settings.Instance["operator_nick"], "-py used by " + command.Sender.Nick + ": " + command.FullArgument);
			}
			if (command.FullArgument != null && (command.FullArgument.ToLower().Contains("ircinterface") || command.FullArgument.ToLower().Contains("datafunctionset"))) {
				ircInterface.SendMessage(command.Channel, "Access to my guts is restricted to the operator.");
				return false;
			} if (command.FullArgument != null && (command.FullArgument.Contains("System.Diagnostics.Process"))) {
				ircInterface.SendMessage(command.Channel, "Process control is restricted to the operator.");
				return false;
			}
			return true;
		}
		public void Use(CommandArgs command)
		{
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
				ircInterface.SendMessage(command.Channel, "Usage: -py [python code] - Leave the [python code] parameter out if you want to close the last indented block of a multi-line script.");
				return;			
			}
			var source = engine.CreateScriptSourceFromString(code, SourceCodeKind.SingleStatement);
			try {
				source.Execute(scope);
			} catch (IronPython.Runtime.UnboundNameException e) {
				ircInterface.SendMessage(command.Channel, "Error: " + e.Message);
				return;
			} catch (SyntaxErrorException e) {
				ircInterface.SendMessage(command.Channel, "Syntax Error: " + e.Message);
				return;
			} catch (IronPython.Runtime.Exceptions.ImportException e) {
				ircInterface.SendMessage(command.Channel, "Import Error: " + e.Message);
				return;
			} catch (MissingMemberException e) {
				ircInterface.SendMessage(command.Channel, "Missing member: " + e.Message);
				return;
			} catch (DivideByZeroException) {
				ircInterface.SendMessage(command.Channel, "A DivideByZeroException occurred");
				return;
			} catch (Exception e) {
				ircInterface.SendMessage(command.Channel, "Unhandled exception: " + e.GetType().ToString() + ": " + e.Message);
				return;
			}
			string line = outputStreamReader.ReadLine();
			if (line == null) {
				ircInterface.SendMessage(command.Channel, "Done (No result)");
				return;
			}
			for(int i = 0; line != null; i++){
				if (i > 3 && !isOperator) {
					ircInterface.SendMessage(command.Channel, "Spam prevention triggered. Sending more than 4 lines is not allowed.");
					break;
				}
				ircInterface.SendMessage(command.Channel, "--> " + line);
				line = outputStreamReader.ReadLine();
				System.Threading.Thread.Sleep(250);
			}
		}
	}
}
