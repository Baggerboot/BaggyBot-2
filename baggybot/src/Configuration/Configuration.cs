namespace BaggyBot.Configuration
{
	public class Configuration
	{
		public bool DebugMode { get; set; } = false;
		public bool LogPerformance { get; set; } = false;
		public int FloodLimit { get; set; } = 4;
		public string StatsPage { get; set; } = "None configured";

		public Interpreters Interpreters { get; private set; } = new Interpreters();
		public Backend[] Backends { get; private set; } = new Backend[0];
		public Integrations Integrations { get; private set; } = new Integrations();
		public Quotes Quotes { get; private set; } = new Quotes();
		public Logging Logging { get; private set; } = new Logging();
		public Metadata Metadata { get; private set; } = new Metadata();

		public Identity[] Identities { get; set; } = new Identity[0];
		public Operator[] Operators { get; set; } = new Operator[0];
		public ServerCfg[] Servers { get; set; } = new ServerCfg[0];
	}
}