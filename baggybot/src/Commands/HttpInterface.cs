using System.IO;
using System.Net;
using System.Text;
using BaggyBot.CommandParsing;
using BaggyBot.Formatting;

namespace BaggyBot.Commands
{
	internal class HttpInterface : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "http";
		public override string Usage => "<method> <URL> [request body ...]";
		public override string Description => "Performs an HTTP request against the given URL, using the given method. You may optionally specify the body of a request as well.";

		public override void Use(CommandArgs command)
		{
			if (command.Args.Length == 0)
			{
				InformUsage(command);
				return;
			}
			var parser = new CommandParser(new Operation()
				.AddArgument("method", "GET")
				.AddArgument("url")
				.AddFlag("follow-redirects", 'f')
				.AddFlag("show-headers", 'h')
				.AddFlag("preserve-newlines", 'n')
				.AddKey("content-type", 'c')
				.AddRestArgument());

			var cmd = parser.Parse(command.FullArgument);

			var url = cmd.Arguments["url"];
			if (!url.Contains("://"))
			{
				url = "http://" + url;
			}

			var request = WebRequest.CreateHttp(url);
			request.Method = cmd.Arguments["method"].ToUpper();
			request.AllowAutoRedirect = cmd.Flags["follow-redirects"];
			request.ContentType = cmd.Keys["content-type"] ?? request.ContentType;
			if (cmd.RestArgument != null)
			{
				using (var sw = new StreamWriter(request.GetRequestStream()))
				{
					sw.Write(cmd.RestArgument);
				}
			}
			HttpWebResponse response;
			try
			{
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (WebException e)
			{
				response = (HttpWebResponse)e.Response;
			}
			if (response == null)
			{
				command.Reply("failed to connect to the server.");
			}
			else
			{
				var sb = new StringBuilder();
				if (cmd.Flags["show-headers"] || request.Method == "HEAD")
				{
					foreach (var header in response.Headers.AllKeys)
					{
						sb.AppendLine($"{header}: {response.Headers[header]}");
					}
				}
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					var text = reader.ReadToEnd();
					if (string.IsNullOrEmpty(text))
					{
						sb.AppendLine("(Empty response)");
					}
					else
					{
						sb.AppendLine(text);
					}
				}
				if (Client.Capabilities.AllowsMultilineMessages || cmd.Flags["preserve-newlines"])
				{
					command.ReturnMessage($"{Frm.MMultiline}{sb}{Frm.MMultiline}");
				}
				else
				{
					command.ReturnMessage(sb.Replace("\n", "").Replace("\r", "\n").ToString());
				}
			}
		}
	}
}
