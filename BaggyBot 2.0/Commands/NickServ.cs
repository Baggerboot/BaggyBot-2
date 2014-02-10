using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.Database;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	public class NickServ : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public NickServ(DataFunctionSet df, IrcInterface inter)
		{
			dataFunctionSet = df;
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 2 && command.Args[0].Equals("add")) {
				if (Tools.UserTools.Validate(command.Sender)) {

					int uid = dataFunctionSet.GetIdFromNick(command.Args[1]);
					if (uid < 0) uid = dataFunctionSet.GetIdFromUser(ircInterface.DoWhoisCall(command.Args[1]));
					string nickserv = ircInterface.DoNickservCall(command.Args[1]);
					dataFunctionSet.SetNsLogin(uid, nickserv);

					command.ReturnMessage("Nickserv updated to {0} for {1}.", nickserv, command.Args[1]);
					return;
				} else {
					command.ReturnMessage(Messages.CMD_NOT_AUTHORIZED);
					return;
				}
			}else if (command.Args.Length == 1 && command.Args[0].Equals("-f")) {
				command.ReturnMessage("Sending a NickServ call");
				string username = ircInterface.DoNickservCall(command.Sender.Nick);
				if (username == null) {
					command.Reply("you don't appear to have registered with NickServ");
				} else {
					command.Reply("your NickServ username is " + username);
					return;
				}
			} else if (command.Args.Length != 0) {
				command.Reply("Usage: -ns; -ns add <username>");
				return;
			} 
			string ns = dataFunctionSet.GetNickserv(dataFunctionSet.GetIdFromUser(command.Sender));
			if (ns == null) {
				command.Reply("you don't appear to be identified with NickServ.");
			} else {
				command.Reply("your NickServ username is " + ns);
			}
		}
	}
}
