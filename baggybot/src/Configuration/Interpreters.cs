using BaggyBot.Commands;

namespace BaggyBot.Configuration
{
	public class Interpreters
	{
		public bool Enabled { get; set; } = true;
		public InterpreterSecurity StartupSecurityLevel { get; set; } = InterpreterSecurity.Block;
	}
}