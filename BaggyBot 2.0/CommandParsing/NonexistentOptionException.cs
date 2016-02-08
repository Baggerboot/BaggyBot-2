using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.CommandParsing
{
	class NonexistentOptionException : InvalidCommandException
	{
		public NonexistentOptionException(string optionName) : base($"The option \"{optionName}\" does not exist.", optionName) { }
	}
}
