namespace BaggyBot.Configuration
{
	public class Quotes
	{
		public double SilentQuoteChance { get; set; } = 0.6;
		public int MinDelayHours { get; set; } = 4;
		public double Chance { get; set; } = 0.015;
		public bool AllowQuoteNotifications { get; set; } = false;
	}
}