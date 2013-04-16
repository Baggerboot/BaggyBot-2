using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			ircInterface.Disconnect();
			sqlConnector.CloseConnection();
			sqlConnector.Dispose();
			Logger.Dispose();
		}
	}
}
