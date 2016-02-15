namespace BaggyBot.CommandParsing
{
	class NonexistentOptionException : InvalidCommandException
	{
		public NonexistentOptionException(string optionName) : base($"The option \"{optionName}\" does not exist.", optionName) { }
	}
}
