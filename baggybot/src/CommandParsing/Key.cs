using System;

namespace BaggyBot.CommandParsing
{
	public class Key : Option
	{
		public object DefaultValue { get; private set; }
		public  Type ValueType { get; }

		public Key(string longForm, object defaultValue, Type valueType, char? shortForm = null) : base(longForm, shortForm)
		{
			DefaultValue = defaultValue;
			ValueType = valueType;
		}
	}
}

