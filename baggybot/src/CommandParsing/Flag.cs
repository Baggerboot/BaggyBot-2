namespace BaggyBot.CommandParsing
{
	public class Flag : Option
	{
		protected Flag(string longForm, char? shortForm = null) : base(longForm, shortForm){ }
	}
}
