using System.IO;
using System.Net;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class Convert : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;

		public void Use(CommandArgs command)
		{
			string fromAmount;
			string fromCurrency;
			string toCurrency;

			switch (command.Args.Length)
			{
				case 2: // -convert 1USD EUR
					if (command.Args[0].Length == 3)
					{
						fromAmount = "1";
					}
					else {
						fromAmount = command.Args[0].Substring(0, command.Args[0].Length - 3);
					}
					fromCurrency = command.Args[0].Substring(command.Args[0].Length - 3, 3);
					toCurrency = command.Args[1];
					break;
				case 3:
					if (command.Args[1].ToLower() == "to")
					{ // -convert 1USD to EUR
						if (command.Args[0].Length == 3)
						{
							fromAmount = "1";
						}
						else {
							fromAmount = command.Args[0].Substring(0, command.Args[0].Length - 3);
						}
						fromCurrency = command.Args[0].Substring(command.Args[0].Length - 3, 3);
						toCurrency = command.Args[2];
					}
					else { // -convert 1 USD EUR
						fromAmount = command.Args[0];
						fromCurrency = command.Args[1];
						toCurrency = command.Args[2];
					}
					break;
				case 4: // -convert 1 USD to EUR
					fromAmount = command.Args[0];
					fromCurrency = command.Args[1];
					toCurrency = command.Args[3];
					break;
				default:
					command.Reply("Usage: -convert <amount> <fromcurrency> to <tocurrency>");
					return;
			}

			var rq = WebRequest.Create($"http://rate-exchange.appspot.com/currency?from={fromCurrency}&to={toCurrency}");
			var response = rq.GetResponse();

			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				dynamic jsonObj = JObject.Parse(sr.ReadToEnd());
				try
				{
					if (jsonObj.to.ToString().ToUpper() != toCurrency.ToUpper() || jsonObj.from.ToString().ToUpper() != fromCurrency.ToUpper())
					{
						command.ReturnMessage("Warning: currency mismatch between {0} to {1} and {2} to {3}", fromCurrency, toCurrency, jsonObj.from.ToString(), jsonObj.to.ToString());
					}
				}
				catch (RuntimeBinderException)
				{
					command.ReturnMessage("Unable to convert between {0} and {1}. Are you sure these are valid currency codes?", fromCurrency, toCurrency);
					return;
				}

				var amount = decimal.Parse(fromAmount);
				decimal rate = decimal.Parse(jsonObj.rate.ToString());
				command.Reply("{1} {2} = {3} {4}", amount, jsonObj.from.ToString().ToUpper(), amount * rate, jsonObj.to.ToString().ToUpper());
			}
		}
	}
}
