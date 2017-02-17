using System;
using System.IO;
using System.Linq;
using System.Net;
using BaggyBot.Configuration;
using Newtonsoft.Json;

namespace BaggyBot
{
	class Bing
	{
		public WebSearchResponse.PageResult WebSearch(string query)
		{
			var encoded = WebUtility.UrlEncode(query);
			var rs = Get<WebSearchResponse>($"https://api.cognitive.microsoft.com/bing/v5.0/search?q={encoded}&mkt=en-US");
			var page = rs.webPages.value.FirstOrDefault();
			return page;
		}

		public ImageSearchResponse.ImageResult ImageSearch(string query)
		{
			var encoded = WebUtility.UrlEncode(query);
			var rs = Get<ImageSearchResponse>($"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={encoded}&mkt=en-US&count=1");

			var image = rs?.value?.FirstOrDefault();
			return image;
		}

		private T Get<T>(string url)
		{
			var response = Get(url);
			var obj = JsonConvert.DeserializeObject<T>(response);
			return obj;
		}

		private string Get(string url)
		{
			var rq = WebRequest.CreateHttp(url);
			rq.Method = "GET";
			rq.ContentType = "application/json";
			rq.Headers["Ocp-Apim-Subscription-Key"] = ConfigManager.Config.Integrations.Bing.Search.Key;
			var response = rq.GetResponse();
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var responseText = reader.ReadToEnd();
				return responseText;
			}
		}


		public class WebSearchResponse
		{
			public string _type { get; set; }
			public Webpages webPages { get; set; }
			public News news { get; set; }
			public Relatedsearches relatedSearches { get; set; }
			public Rankingresponse rankingResponse { get; set; }


			public class Webpages
			{
				public string webSearchUrl { get; set; }
				public int totalEstimatedMatches { get; set; }
				public PageResult[] value { get; set; }
			}

			public class PageResult
			{
				public string id { get; set; }
				public string name { get; set; }
				public string url { get; set; }
				public string displayUrl { get; set; }
				public string snippet { get; set; }
				public DateTime dateLastCrawled { get; set; }
			}

			public class News
			{
				public string id { get; set; }
				public string readLink { get; set; }
				public Value1[] value { get; set; }
			}

			public class Value1
			{
				public string name { get; set; }
				public string url { get; set; }
				public Image image { get; set; }
				public string description { get; set; }
				public About[] about { get; set; }
				public Provider[] provider { get; set; }
				public DateTime datePublished { get; set; }
				public string category { get; set; }
			}

			public class Image
			{
				public string contentUrl { get; set; }
				public Thumbnail thumbnail { get; set; }
			}

			public class Thumbnail
			{
				public string contentUrl { get; set; }
				public int width { get; set; }
				public int height { get; set; }
			}

			public class About
			{
				public string readLink { get; set; }
				public string name { get; set; }
			}

			public class Provider
			{
				public string _type { get; set; }
				public string name { get; set; }
			}

			public class Relatedsearches
			{
				public string id { get; set; }
				public Value2[] value { get; set; }
			}

			public class Value2
			{
				public string text { get; set; }
				public string displayText { get; set; }
				public string webSearchUrl { get; set; }
			}

			public class Rankingresponse
			{
				public Mainline mainline { get; set; }
				public Sidebar sidebar { get; set; }
			}

			public class Mainline
			{
				public Item[] items { get; set; }
			}

			public class Item
			{
				public string answerType { get; set; }
				public int resultIndex { get; set; }
				public Value3 value { get; set; }
			}

			public class Value3
			{
				public string id { get; set; }
			}

			public class Sidebar
			{
				public Item1[] items { get; set; }
			}

			public class Item1
			{
				public string answerType { get; set; }
				public Value4 value { get; set; }
			}

			public class Value4
			{
				public string id { get; set; }
			}
		}

		/*
		public class ImageSearchResponse
		{
			public string _type { get; set; }
			public Webpages webPages { get; set; }
			public Images images { get; set; }
			public Relatedsearches relatedSearches { get; set; }
			public Videos videos { get; set; }
			public Rankingresponse rankingResponse { get; set; }


			public class Webpages
			{
				public string webSearchUrl { get; set; }
				public int totalEstimatedMatches { get; set; }
				public Value[] value { get; set; }
			}

			public class Value
			{
				public string id { get; set; }
				public string name { get; set; }
				public string url { get; set; }
				public About[] about { get; set; }
				public string displayUrl { get; set; }
				public string snippet { get; set; }
				public DateTime dateLastCrawled { get; set; }
			}

			public class About
			{
				public string name { get; set; }
			}

			public class Images
			{
				public string id { get; set; }
				public string readLink { get; set; }
				public string webSearchUrl { get; set; }
				public bool isFamilyFriendly { get; set; }
				public ImageResult[] value { get; set; }
				public bool displayShoppingSourcesBadges { get; set; }
				public bool displayRecipeSourcesBadges { get; set; }
			}

			public class ImageResult
			{
				public string name { get; set; }
				public string webSearchUrl { get; set; }
				public string thumbnailUrl { get; set; }
				public object datePublished { get; set; }
				public string contentUrl { get; set; }
				public string hostPageUrl { get; set; }
				public string contentSize { get; set; }
				public string encodingFormat { get; set; }
				public string hostPageDisplayUrl { get; set; }
				public int width { get; set; }
				public int height { get; set; }
				public Thumbnail thumbnail { get; set; }
			}

			public class Thumbnail
			{
				public int width { get; set; }
				public int height { get; set; }
			}

			public class Relatedsearches
			{
				public string id { get; set; }
				public Value2[] value { get; set; }
			}

			public class Value2
			{
				public string text { get; set; }
				public string displayText { get; set; }
				public string webSearchUrl { get; set; }
			}

			public class Videos
			{
				public string id { get; set; }
				public string readLink { get; set; }
				public string webSearchUrl { get; set; }
				public bool isFamilyFriendly { get; set; }
				public Value3[] value { get; set; }
				public string scenario { get; set; }
			}

			public class Value3
			{
				public string name { get; set; }
				public string description { get; set; }
				public string webSearchUrl { get; set; }
				public string thumbnailUrl { get; set; }
				public DateTime datePublished { get; set; }
				public Publisher[] publisher { get; set; }
				public string contentUrl { get; set; }
				public string hostPageUrl { get; set; }
				public string encodingFormat { get; set; }
				public string hostPageDisplayUrl { get; set; }
				public int width { get; set; }
				public int height { get; set; }
				public string duration { get; set; }
				public string motionThumbnailUrl { get; set; }
				public string embedHtml { get; set; }
				public bool allowHttpsEmbed { get; set; }
				public int viewCount { get; set; }
				public Thumbnail1 thumbnail { get; set; }
				public bool allowMobileEmbed { get; set; }
				public bool isSuperfresh { get; set; }
			}

			public class Thumbnail1
			{
				public int width { get; set; }
				public int height { get; set; }
			}

			public class Publisher
			{
				public string name { get; set; }
			}

			public class Rankingresponse
			{
				public Mainline mainline { get; set; }
				public Sidebar sidebar { get; set; }
			}

			public class Mainline
			{
				public Item[] items { get; set; }
			}

			public class Item
			{
				public string answerType { get; set; }
				public Value4 value { get; set; }
				public int resultIndex { get; set; }
			}

			public class Value4
			{
				public string id { get; set; }
			}

			public class Sidebar
			{
				public Item1[] items { get; set; }
			}

			public class Item1
			{
				public string answerType { get; set; }
				public Value5 value { get; set; }
			}

			public class Value5
			{
				public string id { get; set; }
			}
		}
		*/


		public class ImageSearchResponse
		{
			public string _type { get; set; }
			public Instrumentation instrumentation { get; set; }
			public string readLink { get; set; }
			public string webSearchUrl { get; set; }
			public int totalEstimatedMatches { get; set; }
			public ImageResult[] value { get; set; }
			public Queryexpansion[] queryExpansions { get; set; }
			public int nextOffsetAddCount { get; set; }
			public Pivotsuggestion[] pivotSuggestions { get; set; }
			public bool displayShoppingSourcesBadges { get; set; }
			public bool displayRecipeSourcesBadges { get; set; }

			public class Instrumentation
			{
				public string pageLoadPingUrl { get; set; }
			}

			public class ImageResult
			{
				public string name { get; set; }
				public string webSearchUrl { get; set; }
				public string thumbnailUrl { get; set; }
				public DateTime datePublished { get; set; }
				public string contentUrl { get; set; }
				public string hostPageUrl { get; set; }
				public string contentSize { get; set; }
				public string encodingFormat { get; set; }
				public string hostPageDisplayUrl { get; set; }
				public int width { get; set; }
				public int height { get; set; }
				public Thumbnail thumbnail { get; set; }
				public string imageInsightsToken { get; set; }
				public Insightssourcessummary insightsSourcesSummary { get; set; }
				public string imageId { get; set; }
				public string accentColor { get; set; }
			}

			public class Thumbnail
			{
				public int width { get; set; }
				public int height { get; set; }
			}

			public class Insightssourcessummary
			{
				public int shoppingSourcesCount { get; set; }
				public int recipeSourcesCount { get; set; }
			}

			public class Queryexpansion
			{
				public string text { get; set; }
				public string displayText { get; set; }
				public string webSearchUrl { get; set; }
				public string searchLink { get; set; }
				public Thumbnail1 thumbnail { get; set; }
			}

			public class Thumbnail1
			{
				public string thumbnailUrl { get; set; }
			}

			public class Pivotsuggestion
			{
				public string pivot { get; set; }
				public Suggestion[] suggestions { get; set; }
			}

			public class Suggestion
			{
				public string text { get; set; }
				public string displayText { get; set; }
				public string webSearchUrl { get; set; }
				public string searchLink { get; set; }
				public Thumbnail2 thumbnail { get; set; }
			}

			public class Thumbnail2
			{
				public string thumbnailUrl { get; set; }
			}
		}
	}
}
