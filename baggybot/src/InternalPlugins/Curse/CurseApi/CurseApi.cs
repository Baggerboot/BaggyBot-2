using BaggyBot.InternalPlugins.Curse.CurseApi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BaggyBot.InternalPlugins.Curse.CurseApi
{
	class CurseApi
	{
		public string AuthToken { get; set; }

		public T Post<T>(string url, string payload)
		{
			var rq = WebRequest.CreateHttp(url);
			rq.Method = "POST";
			rq.ContentType = "application/x-www-form-urlencoded";
			rq.CookieContainer = new CookieContainer();
			using (var writer = new StreamWriter(rq.GetRequestStream()))
			{
				writer.Write(payload);
			}
			var response = rq.GetResponse();
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
			}
		}

		public T Post<T>(string url, RequestObject obj)
		{
			var rq = WebRequest.CreateHttp(url);
			rq.Method = "POST";
			rq.ContentType = "application/json";
			rq.Headers["AuthenticationToken"] = AuthToken;
			using (var writer = new StreamWriter(rq.GetRequestStream()))
			{
				writer.Write(JsonConvert.SerializeObject(obj));
			}
			var response = rq.GetResponse();
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
			}
		}
	}


	class ResponseObject
	{
		public ResponseObject(string responseText, HttpWebResponse response)
		{
			ResponseText = responseText;
			Response = response;
		}

		public HttpWebResponse Response { get; }
		public string ResponseText { get; }
	}
}
