using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.Database;

namespace BaggyBot.Commands
{
	class Shutdown : ICommand
	{
		private IrcInterface ircInterface;
		private SqlConnector sqlConnector;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Shutdown(IrcInterface inter, SqlConnector sc)
		{
			ircInterface = inter;
			sqlConnector = sc;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.Disconnect("Shutting down");
			sqlConnector.CloseConnection();
			sqlConnector.Dispose();
			Logger.Dispose();
			Environment.Exit(0);
		}
	}
}
