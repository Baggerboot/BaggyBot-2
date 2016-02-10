using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;

namespace BaggyBot.Commands.Interpreters.Roslyn
{
	class RoslynInterpreter
	{
		private ScriptState state;

		public RoslynInterpreter()
		{
			var options = ScriptOptions.Default
				.AddReferences(Assembly.GetAssembly(typeof(RoslynInterpreter)))
				.AddImports("System", "System.Math", "System.Linq", "System.Collections.Generic", "System.IO", "System.Reflection", "System.Dynamic", "BaggyBot", "BaggyBot.DataProcessors.IO");
			var script = CSharpScript.Create("", options, typeof(InterpreterGlobals));
			state = script.RunAsync(InterpreterContext.Globals).Result;
		}

		public string Interpret(string code)
		{
			try
			{
				state = state.ContinueWithAsync(code).Result;
				var returnValue = state.ReturnValue;
				if (returnValue == null)
				{
					return "--> (null)";
				}
				else
				{
					return "--> " + CodeFormatter.PrettyPrint(returnValue);
				}
				
			}
			catch (CompilationErrorException e) when (e.Message.Contains("CS1002"))
			{
				try
				{
					state = state.ContinueWithAsync(code + ";").Result;
					var returnValue = state.ReturnValue;
					return "--> " + (returnValue?.ToString() ?? "(null)");
				}
				catch (Exception e2)
				{
					return e2.Message;
				}
			}
			catch (AggregateException e)
			{
				if (e.InnerExceptions.Count == 1)
				{
					return e.InnerExceptions.First().Message;
				}
				else
				{
					return "Multiple errors occurred while trying to evaluate your expression. You should probably sit back and contemplate the code you've just written.";
				}
			}
			catch (Exception e) when (e is CompilationErrorException || e is RuntimeBinderException)
			{
				return e.Message;
			}
		}
	}
}
