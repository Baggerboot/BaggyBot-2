using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.CommandParsing
{
	class Argument
	{
		public string Name { get; private set; }
		public string DefaultValue { get; private set; }

		public Argument(string name, string defaultValue)
		{
			Name = name;
			DefaultValue = defaultValue;
		}
	}
}
