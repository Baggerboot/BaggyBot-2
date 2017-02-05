namespace BaggyBot.CommandParsing
{
	class Argument
	{
		public string Name { get; private set; }
		public string DefaultValue { get; private set; }
		public bool Required { get; private set; }

		public Argument(string name, string defaultValue)
		{
			Name = name;
			DefaultValue = defaultValue;
		}

		public Argument(string name)
		{
			Name = name;
			Required = true;
		}
	}
}
