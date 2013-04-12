﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Crash : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;

		public Crash(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(Command command)
		{
			if (dataFunctionSet.GetNickserv(dataFunctionSet.GetIdFromUser(command.Sender)).Equals("Baggerboot")) {
				throw new Exception("Manually initiated crash.");
			}
		}
	}
}
