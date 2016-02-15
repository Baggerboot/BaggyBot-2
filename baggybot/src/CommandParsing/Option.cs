namespace BaggyBot.CommandParsing
{
	public abstract class Option
	{
		public char? Short { get; private set; }
		public string Long { get; private set; }
		
		protected Option(string longForm, char ? shortForm = null)
		{
			Short = shortForm;
			Long = longForm;
		}
	}
}
