using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.CommandParsing
{
	class InvalidCommandException : ArgumentException
	{
		public InvalidCommandException(string message, string argument) : base(message, argument) { }
	}
}
