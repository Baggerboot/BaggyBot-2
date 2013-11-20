using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Approve : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Approve(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			// TODO: Add implementation for approve command
		}
	}
}
