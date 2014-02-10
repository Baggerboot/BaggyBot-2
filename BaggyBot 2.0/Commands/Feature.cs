using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Feature : ICommand
	{
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Feature(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			string search = command.FullArgument;

			var searchResults  = dataFunctionSet.FindQuote(search);

			if(searchResults == null){
				command.ReturnMessage("No such quote found.");
				return;
			}

			StringBuilder quoteListBuiler = new StringBuilder();
			quoteListBuiler.Append("Multiple quotes found: ");

			int max = searchResults.Count > 12 ? 12 : searchResults.Count;

			for(int i = 0; i < max; i++){
				quoteListBuiler.Append("\"");
				quoteListBuiler.Append(searchResults[i].Quote1.Substring(0,25));
				quoteListBuiler.Append("\"");
				if( i != max -1){
					quoteListBuiler.Append(", ");
				}
			}
			int diff = searchResults.Count - max;
			if (diff > 0) {
				quoteListBuiler.Append(String.Format(" and {0} more.", diff));
			}


			if (searchResults.Count > 1) {
				command.Reply(quoteListBuiler.ToString());
				return;
			} else {
				command.ReturnMessage("The following quote has been featured: \"" + searchResults[0].Quote1 + "\"");
				dataFunctionSet.SetVar("featured_quote", searchResults[0].ID);
			}
		}
	}
}
