using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaggyBot.Database;

namespace BaggyBot.Commands
{
	public class NickServ : ICommand
	{
		private IrcInterface ircInterface;
		private DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public NickServ(IrcInterface inter, DataFunctionSet df)
		{
			ircInterface = inter;
			dataFunctionSet = df;
		}

		public void Use(CommandArgs c)
		{
			if (c.Args.Length == 2 && c.Args[0].Equals("add")) {
				if (Tools.UserTools.Validate(c.Sender)) {

					int uid = dataFunctionSet.GetIdFromNick(c.Args[1]);
					if (uid < 0) uid = dataFunctionSet.GetIdFromUser(ircInterface.DoWhoisCall(c.Args[1]));
					string nickserv = ircInterface.DoNickservCall(c.Args[1]);
					dataFunctionSet.SetNsLogin(uid, nickserv);

					ircInterface.SendMessage(c.Channel, string.Format("Nickserv updated to {0} for {1}.", nickserv, c.Args[1]));
					return;
				} else {
					ircInterface.SendMessage(c.Channel, Messages.CMD_NOT_AUTHORIZED);
					return;
				}
			} else if (c.Args.Length != 0) {
				ircInterface.SendMessage(c.Channel, "Usage: -ns; -ns add <username>");
				return;
			}
			string ns = dataFunctionSet.GetNickserv(dataFunctionSet.GetIdFromUser(c.Sender));
			if (ns == null) {
				ircInterface.SendMessage(c.Channel, "You don't appear to be identified with NickServ.");
			} else {
				ircInterface.SendMessage(c.Channel, "Your NickServ username is " + ns);
			}
		}
	}
}
