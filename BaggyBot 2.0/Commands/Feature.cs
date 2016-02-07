using System;
using System.Text;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	internal class Feature : ICommand
	{
		private readonly DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions => PermissionLevel.BotOperator;

		public Feature(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			var search = command.FullArgument;

			var searchResults = dataFunctionSet.FindQuote(search);

			if (searchResults == null)
			{
				command.ReturnMessage("No such quote found.");
				return;
			}

			var quoteListBuiler = new StringBuilder();
			quoteListBuiler.Append("Multiple quotes found: ");

			var max = searchResults.Count > 12 ? 12 : searchResults.Count;

			for (var i = 0; i < max; i++)
			{
				quoteListBuiler.Append("\"");
				quoteListBuiler.Append(searchResults[i].Text.Substring(0, 25));
				quoteListBuiler.Append("\"");
				if (i != max - 1)
				{
					quoteListBuiler.Append(", ");
				}
			}
			var diff = searchResults.Count - max;
			if (diff > 0)
			{
				quoteListBuiler.Append($" and {diff} more.");
			}
			
			if (searchResults.Count > 1)
			{
				command.Reply(quoteListBuiler.ToString());
				return;
			}
			command.ReturnMessage("The following quote has been featured: \"" + searchResults[0].Text + "\"");
			dataFunctionSet.SetVar("featured_quote", searchResults[0].Id);
		}
	}
}
