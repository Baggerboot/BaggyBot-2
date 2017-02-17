using System.IO;
using System.Net;
using BaggyBot.Configuration;
using Newtonsoft.Json;

namespace BaggyBot.ExternalApis
{
	internal class Imgur
	{
		public static string Upload(string imageUrl, string title = null, string description = null)
		{
			var result = Post<UploadResponse>($"https://api.imgur.com/3/image?type=url&image={imageUrl}&title={title}&description={description}");
			return result?.data?.link;
		}

		private static T Post<T>(string url)
		{
			return JsonConvert.DeserializeObject<T>(Post(url));
		}

		private static string Post(string url)
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

		internal class UploadResponse
		{
			public Data data { get; set; }
			public bool success { get; set; }
			public int status { get; set; }
		}

		internal class Data
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
