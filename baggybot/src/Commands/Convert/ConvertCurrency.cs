using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Monitoring;
using BaggyBot.Tools;
using Newtonsoft.Json.Linq;

namespace BaggyBot.Commands
{
	internal class ConvertCurrency : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "convert";
		public override string Usage => "<amount> <ISO Currency Code> [to] <ISO Currency Code>";
		public override string Description => "Converts the value of a given amount of money from one currency to another.";
		private readonly Timer timer;
		private Dictionary<string, decimal> exchangeRates;
		private const int MAX_LOOKUP_ATTEMPTS = 5;

		public ConvertCurrency()
		{
			timer = new Timer(UpdateExchangeRate, null, TimeSpan.FromSeconds(1), TimeSpan.FromHours(2));
		}

		private void UpdateExchangeRate(object stateInfo)
		{
			RequestExchangeRates(1);
		}

		private void RequestExchangeRates(int attemptNumber)
		{
			if (attemptNumber > MAX_LOOKUP_ATTEMPTS) return;
			try
			{
				Logger.Log(this, "Looking up the latest currency exchange rates");
				var jsonObj = MiscTools.GetJson($"http://api.fixer.io/latest?base=EUR");
				exchangeRates = new Dictionary<string, decimal>();
				foreach (var prop in jsonObj["rates"].Children().Cast<JProperty>())
				{
					exchangeRates[prop.Name] = (decimal)prop.Value;
				}
				Logger.Log(this, $"Exchange rates for {exchangeRates.Count + 1} currencies have been updated.");
			}
			catch (WebException e) when (e.Message == "The request timed out" || e.Message == "The remote server returned an error: (500) Internal Server Error.")
			{
				Logger.Log(this, $"A server error occurred while updating the currency exhange rates. Retrying in {attemptNumber} seconds...");
				Task.Delay(TimeSpan.FromSeconds(attemptNumber)).ContinueWith(_ => RequestExchangeRates(++attemptNumber));
			}
			catch (Exception e)
			{
				Logger.LogException(this, e, "updating the currency exchange rates");
				RequestExchangeRates(++attemptNumber);
			}
		}

		public override void Use(CommandArgs command)
		{
			if (command.FullArgument == null)
			{
				InformUsage(command);
				return;
			}
			var match = Regex.Match(command.FullArgument, @"^((?:\d*(?:\.|,)\d+)|(?:\d+))\s*([a-z]{3}).*\s([a-z]{3})$", RegexOptions.IgnoreCase);
			if (!match.Success)
			{
				InformUsage(command);
				return;
			}
			decimal fromAmount;
			if (!decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out fromAmount))
			{
				command.Reply($"I don't know how to turn {match.Groups[1].Value} into a number.");
				return;
			}
			if (exchangeRates == null)
			{
				command.Reply("please wait a moment, I'm still looking up the exchange rates.");
				return;
			}
			var fromCurrency = match.Groups[2].Value.ToUpper();
			var toCurrency = match.Groups[3].Value.ToUpper();

			if (toCurrency != "EUR" && !exchangeRates.ContainsKey(toCurrency))
			{
				command.Reply($"I don't know the exchange rate of {toCurrency}");
				return;
			}
			if (fromCurrency != "EUR" && !exchangeRates.ContainsKey(fromCurrency))
			{
				command.Reply($"I don't know the exchange rate of {fromCurrency}");
				return;
			}
			decimal result;
			// The base currency is EUR, so if we're converting to or from EUR, no additional conversion is necessary.
			if (fromCurrency == "EUR")
			{
				result = fromAmount*exchangeRates[toCurrency];
			}
			else if (toCurrency == "EUR")
			{
				result = fromAmount/exchangeRates[fromCurrency];
			}
			// First convert from the source currency to EUR, then convert from EUR to the target currency.
			else
			{
				result = fromAmount/exchangeRates[fromCurrency]*exchangeRates[toCurrency];
			}
			command.Reply($"{fromAmount} {fromCurrency} = {result:F} {toCurrency}");
		}

		public override void Dispose()
		{
			base.Dispose();
			timer.Dispose();
		}
	}
}
