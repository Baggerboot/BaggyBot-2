using System;
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
				var title = WebUtility.UrlEncode(image.name);
				var descr = WebUtility.UrlEncode("From " + image.hostPageDisplayUrl);

				string finalUrl;
				// Bing refuses to serve out the original URL, so we reupload it to Imgur.
				// An alternative option would be to run a HEAD on the image URL to get the redirect URL.
				if (Client.Capabilities.RequireReupload)
				{
					var imageUrl = WebUtility.UrlEncode(image.contentUrl);
					finalUrl = Imgur.Upload(imageUrl, title, descr);
				}
				else
				{
					try
					{
						finalUrl = FollowAllRedirects(image.contentUrl);
					}
					catch (Exception)
					{
						finalUrl = null;
					}
				}
				command.Reply($"{finalUrl ?? image.contentUrl}");
			}
		}

		private static string FollowAllRedirects(string url)
		{
			var rq = WebRequest.CreateHttp(url);
			rq.Method = "HEAD";

			HttpWebResponse rs;
			try
			{
				rs = (HttpWebResponse) rq.GetResponse();
			}
			catch (WebException e)
			{
				rs = (HttpWebResponse) e.Response;
			}
			if (rs == null)
			{
				throw new InvalidOperationException("URL is not accessible");
			}
			int statusCode = (int)rs.StatusCode;
			// non-300 returned, so the content can be requested directly from this URL.
			if (statusCode < 300 || statusCode >= 400) return rs.ResponseUri.ToString();
			// Check if the URL indicated by the Location header has any further redirects.
			return FollowAllRedirects(rs.Headers["Location"]);
		}
	}
}
