namespace BaggyBot.CommandParsing
{
	struct CommandComponent
	{
		public int Position;
		public string Value;

		public CommandComponent(int position, string value)
		{
			Position = position;
			Value = value;
		}
	}
}
