using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class ExceptionDetails : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.BotOperator; } }

		public ExceptionDetails(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			bool remove = false;
			int index = 0;
			for (int i = 0; i < command.Args.Length; i++) {
				switch (command.Args[i]) {
					case "-ra":
						Program.Exceptions.Clear();
						ircInterface.SendMessage(command.Channel, "All exceptions removed.");
						return;
					case "-r":
						remove = true;
						break;
					case "-i":
						index = int.Parse(command.Args[i + 1]);
						i++;
						break;
				}
			}

			if (Program.Exceptions.Count == 0) {
				ircInterface.SendMessage(command.Channel, "No exceptions left.");
				return;
			} else if (Program.Exceptions.Count <= index) {
				ircInterface.SendMessage(command.Channel, "There is no exception with that ID");
				return;
			}

			Exception e = Program.Exceptions[index];
			if(remove) Program.Exceptions.RemoveAt(index);
			ircInterface.SendMessage(command.Channel, string.Format("Exception #{0}: \"{1}\"", index, e.Message));
		}
	}
}
