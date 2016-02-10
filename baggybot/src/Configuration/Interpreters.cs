using BaggyBot.Commands;

namespace BaggyBot.Configuration
{
	public class Interpreters
	{
		public bool Enabled { get; set; } = true;
		public ReadEvaluatePrintCommand.InterpreterSecurity StartupSecurityLevel { get; set; } = ReadEvaluatePrintCommand.InterpreterSecurity.Block;
	}
}