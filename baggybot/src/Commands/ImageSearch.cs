using System.IO;
using System.Net;
using BaggyBot.Configuration;
using Newtonsoft.Json;

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

				var result = Post<UploadResponse>($"https://api.imgur.com/3/image?type=url&image={imageUrl}&title={title}&description={descr}");


				command.Reply($"{result?.data?.link ?? image.contentUrl}");
			}
		}
		private T Post<T>(string url)
		{
			return JsonConvert.DeserializeObject<T>(Post(url));
		}

		private string Post(string url)
		{
			var rq = WebRequest.CreateHttp(url);
			rq.Method = "POST";
			//rq.ContentType = "application/json";
			rq.Headers["Authorization"] = "Client-ID " + ConfigManager.Config.Integrations.Imgur.ClientId;
			var response = rq.GetResponse();
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var responseText = reader.ReadToEnd();
				return responseText;
			}
		}

		public class UploadResponse
		{
			public Data data { get; set; }
			public bool success { get; set; }
			public int status { get; set; }
		}

		public class Data
		{
			public string id { get; set; }
			public string title { get; set; }
			public string description { get; set; }
			public int datetime { get; set; }
			public string type { get; set; }
			public bool animated { get; set; }
			public int width { get; set; }
			public int height { get; set; }
			public int size { get; set; }
			public int views { get; set; }
			public int bandwidth { get; set; }
			public object vote { get; set; }
			public bool favorite { get; set; }
			public object nsfw { get; set; }
			public object section { get; set; }
			public object account_url { get; set; }
			public int account_id { get; set; }
			public bool is_ad { get; set; }
			public object[] tags { get; set; }
			public bool in_gallery { get; set; }
			public string deletehash { get; set; }
			public string name { get; set; }
			public string link { get; set; }
		}

	}
}
