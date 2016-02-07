namespace BaggyBot
{
	// TODO: Globally modifiable state is evil, fix this.
	public static class ControlVariables
	{
		public static bool SnagNextLine { get; set; }
		public static string SnagNextLineBy { get; set; }
		public static bool QueryConsole { get; set; }
	}
}
