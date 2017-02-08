namespace BaggyBot.Configuration
{
	public class Integrations
	{
		public WolframAlpha WolframAlpha { get; set; } = new WolframAlpha();
		public Bing Bing { get; set; } = new Bing();
		public Imgur Imgur { get; set; }= new Imgur();
	}
}