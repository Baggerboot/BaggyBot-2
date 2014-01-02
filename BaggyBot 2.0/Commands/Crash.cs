using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Crash : ICommand
	{
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public Crash(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			throw new Exception("Manually initiated crash.");
		}
	}
}
