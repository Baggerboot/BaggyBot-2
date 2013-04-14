using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Query : ICommand
	{
		private IrcInterface ircInterface;
		private SqlConnector sqlConnector;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Query(IrcInterface inter, SqlConnector sc)
		{
			ircInterface = inter;
			sqlConnector = sc;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 0) {
				ircInterface.SendMessage(command.Channel, "Usage: -query <SQL query>");
				return;
			}

			string query = "";
			foreach (string var in command.Args) {
				query += (" " + var);
			}
			query = query.Substring(1);

			if (command.Args[0].ToUpper().Equals("SELECT")) {
				

				Object[] results = sqlConnector.SelectVector<Object>(query);
				string resultLine = "";
				foreach (Object o in results) {
					resultLine += (" " + o.ToString());
				}
				resultLine = resultLine.Substring(1);
				ircInterface.SendMessage(command.Channel, command.Sender.Nick + ": " + resultLine);
			} else {
				int result = sqlConnector.ExecuteStatement(query);
				string multiple = result == 1 ? "s" : "";
				ircInterface.SendMessage(command.Channel, String.Format("{1} row{2} affected.", command.Sender.Nick, result, multiple));
			}
		}
	}
}

