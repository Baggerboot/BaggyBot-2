using BaggyBot.EmbeddedData;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	internal class NickServ : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Usage => "[-f]|[set <username>]";
		public override string Description => "Performs a NickServ lookup for your username against the database. Use `-f` to query the NickServ service instead, or use `set` to store a user's current NickServ username in the database.";
		
		public override void Use(CommandArgs command)
		{
			if (command.Args.Length == 2 && command.Args[0].Equals("set"))
			{
				if (UserTools.Validate(command.Sender))
				{
					var uid = command.Client.StatsDatabase.GetIdFromNick(command.Args[1]);
					if (uid < 0) uid = command.Client.StatsDatabase.GetIdFromUser(command.Client.DoWhoisCall(command.Args[1]));
					var nickserv = command.Client.NickservLookup(command.Args[1]);
					command.Client.StatsDatabase.SetNsLogin(uid, nickserv.AccountName);

					command.ReturnMessage("Nickserv updated to {0} for {1}.", nickserv, command.Args[1]);
					return;
				}
				command.ReturnMessage(Messages.CmdNotAuthorised);
				return;
			}
			if (command.Args.Length == 1 && command.Args[0].Equals("-f"))
			{
				command.ReturnMessage("Sending a NickServ call");
				var username = command.Client.NickservLookup(command.Sender.Nick);
				if (username == null)
				{
					command.Reply("you don't appear to be registered with NickServ");
				}
				else {
					command.Reply("your NickServ username is " + username);
					return;
				}
			}
			else if (command.Args.Length != 0)
			{
				command.Reply("Usage: -ns; -ns add <username>");
				return;
			}
			var ns = command.Client.StatsDatabase.GetNickserv(command.Client.StatsDatabase.GetIdFromUser(command.Sender));
			if (ns == null)
			{
				command.Reply("you don't appear to be identified with NickServ.");
			}
			else {
				command.Reply("your NickServ username is " + ns);
			}
		}
	}
}
