namespace BaggyBot.Configuration
{
	public class Configuration
	{
		public bool DebugMode { get; set; } = false;
		public int FloodLimit { get; set; } = 4;
		public bool LogPerformance { get; set; } = false;
		public string StatsPage { get; set; } = "None configured";

		public Backend[] Backends { get; private set; } = new Backend[0];
		public Interpreters Interpreters { get; private set; } = new Interpreters();
		public Integrations Integrations { get; private set; } = new Integrations();
		public Quotes Quotes { get; private set; } = new Quotes();
		public Logging Logging { get; private set; } = new Logging();

		public Operator[] Operators { get; set; } = new Operator[0];
		public ServerCfg[] Servers { get; set; } = new ServerCfg[0];

		public Metadata Metadata { get; private set; } = new Metadata();
	}
}