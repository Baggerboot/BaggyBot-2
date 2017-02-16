using System.Net;
using BaggyBot.ExternalApis;

namespace BaggyBot.Commands
{
	class ImageSearch : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "gis";
		public override string Usage => "<search query>";
		public override string Description => "Search the web for images.";

		private readonly Bing bing = new Bing();

		public override void Use(CommandArgs command)
		{
			var query = command.FullArgument;
			if (query == null)
			{
				InformUsage(command);
				return;
			}
			var image = bing.ImageSearch(query);
			if (image == null)
			{
				command.Reply("there don't seem to be any results.");
			}
			else
			{
				var imageUrl = WebUtility.UrlEncode(image.contentUrl);
				var title = WebUtility.UrlEncode(image.name);
				var descr = WebUtility.UrlEncode("From " + image.hostPageDisplayUrl);
				// Bing refuses to serve out the original URL, so we reupload it to Imgur.
				// An alternative option would be to run a HEAD on the image URL to get the redirect URL.
				var url = Imgur.Upload(imageUrl, title, descr);
				command.Reply($"{url ?? image.contentUrl}");
			}
		}

	}
}
