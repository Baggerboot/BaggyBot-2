using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Feature : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Feature(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			string search = command.FullArgument;

			var searchResults  = dataFunctionSet.FindQuote(search);

			if(searchResults == null){
				ircInterface.SendMessage(command.Channel, "No such quote found.");
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
				ircInterface.SendMessage(command.Channel, quoteListBuiler.ToString());
				return;
			} else {
				ircInterface.SendMessage(command.Channel, "The following quote has been featured: \"" + searchResults[0].Quote1 + "\"");
				dataFunctionSet.SetVar("featured_quote", searchResults[0].ID);
			}
		}
	}
}
