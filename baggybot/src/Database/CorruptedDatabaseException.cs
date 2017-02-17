using System;

namespace BaggyBot.Database
{
	class CorruptedDatabaseException : Exception
	{
		public CorruptedDatabaseException(string message) : base(message) { }
	}
}
