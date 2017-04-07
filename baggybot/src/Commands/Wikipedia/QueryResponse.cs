using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands.Wikipedia
{
	public class QueryResponse
	{
		public object batchcomplete { get; set; }
		public Query query { get; set; }
	}

	public class Query
	{
		public Normalized[] normalized { get; set; }
		public JObject pages { get; set; }
	}

	public class Page
	{
		public int pageid { get; set; }
		public int ns { get; set; }
		public string title { get; set; }
		public string contentmodel { get; set; }
		public string pagelanguage { get; set; }
		public string pagelanguagehtmlcode { get; set; }
		public string pagelanguagedir { get; set; }
		public DateTime touched { get; set; }
		public int lastrevid { get; set; }
		public int length { get; set; }
		public string redirect { get; set; }
		public string fullurl { get; set; }
		public string editurl { get; set; }
		public string canonicalurl { get; set; }
	}

	public class Normalized
	{
		public string from { get; set; }
		public string to { get; set; }
	}

}
