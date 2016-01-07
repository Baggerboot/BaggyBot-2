using BaggyBot.DataProcessors;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	public class NickServ : ICommand
	{
		private readonly IrcInterface ircInterface;
		private readonly DataFunctionSet dataFunctionSet;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public NickServ(DataFunctionSet df, IrcInterface inter)
		{
			dataFunctionSet = df;
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 2 && command.Args[0].Equals("add")) {
				if (UserTools.Validate(command.Sender)) {

					var uid = dataFunctionSet.GetIdFromNick(command.Args[1]);
					if (uid < 0) uid = dataFunctionSet.GetIdFromUser(ircInterface.DoWhoisCall(command.Args[1]));
					var nickserv = ircInterface.DoNickservCall(command.Args[1]);
					dataFunctionSet.SetNsLogin(uid, nickserv);

					command.ReturnMessage("Nickserv updated to {0} for {1}.", nickserv, command.Args[1]);
					return;
				}
				command.ReturnMessage(Messages.CmdNotAuthorized);
				return;
			}
			if (command.Args.Length == 1 && command.Args[0].Equals("-f")) {
				command.ReturnMessage("Sending a NickServ call");
				var username = ircInterface.DoNickservCall(command.Sender.Nick);
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
			var ns = dataFunctionSet.GetNickserv(dataFunctionSet.GetIdFromUser(command.Sender));
			if (ns == null) {
				command.Reply("you don't appear to be identified with NickServ.");
			} else {
				command.Reply("your NickServ username is " + ns);
			}
		}
	}
}
