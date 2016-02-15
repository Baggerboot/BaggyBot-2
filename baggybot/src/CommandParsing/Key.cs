namespace BaggyBot.CommandParsing
{
	public class Key : Option
	{
		public string DefaultValue { get; private set; }

		public Key(string longForm, string defaultValue, char? shortForm = null) : base(longForm, shortForm)
		{
			DefaultValue = defaultValue;
		}
	}
}
