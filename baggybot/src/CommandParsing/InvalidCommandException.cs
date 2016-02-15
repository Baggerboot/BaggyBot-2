using System;

namespace BaggyBot.CommandParsing
{
	class InvalidCommandException : ArgumentException
	{
		public InvalidCommandException(string message, string argument) : base(message, argument) { }
	}
}
