using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Database
{
	class CorruptedDatabaseException : Exception
	{
		public CorruptedDatabaseException(string message) : base(message) { }
	}
}
