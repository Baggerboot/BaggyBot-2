using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.CommandParsing
{
	public class Flag : Option
	{
		protected Flag(string longForm, char? shortForm = null) : base(longForm, shortForm){ }
	}
}
