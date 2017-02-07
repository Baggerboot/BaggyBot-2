using System;
using BaggyBot.EmbeddedData;
using BaggyBot.Tools;
using IRCSharp.IRC;

namespace BaggyBot.Commands
{
	internal class NickServ : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "nickserv";
		public override string Usage => "[-f]|[update]|[set <username>]";
		public override string Description => "Performs a NickServ lookup for your username against the database. Use `-f` to query the NickServ service instead, or use `set` to store a user's current NickServ username in the database.";

		private NickservInformation NickservLookup(string name)
		{
			// TODO: reimplement nickserv lookup
			throw new NotImplementedException();
		}
		
		// TODO: Candidate for porting to CommandParsing
		public override void Use(CommandArgs command)
		{
			if (command.Args.Length == 2 && command.Args[0] == "set")
			{
				if (Client.Validate(command.Sender))
				{
					var uid = StatsDatabase.GetUserByNickname(command.Args[1]).Id;
					var nickserv = NickservLookup(command.Args[1]);
					StatsDatabase.SetNsLogin(uid, nickserv.AccountName);

					command.ReturnMessage($"Nickserv updated to {nickserv} for {command.Args[1]}.");
					return;
				}
				command.ReturnMessage(Messages.CmdNotAuthorised);
				return;
			}
			if(command.Args.Length == 1 && command.Args[0] == "update")
			{
				var user = StatsDatabase.MapUser(command.Sender);
				var nickserv = NickservLookup(command.Sender.Nickname);
				var rows = StatsDatabase.SetNsLogin(user.Id, nickserv?.AccountName);
				command.Reply($"NickServ reports that your account name is '{nickserv?.AccountName}'. I've updated the database to reflect that ({rows} record(s) affected).");
                return;
			}
			if (command.Args.Length == 1 && command.Args[0] == "-f")
			{
				command.ReturnMessage("Sending a NickServ call");
				var username = NickservLookup(command.Sender.Nickname);
				if (username == null)
				{
					command.Reply("you don't appear to be registered with NickServ");
				}
				else {
					command.Reply("your NickServ username is " + username);
					return;
				}
			}
			if (command.Args.Length != 0)
			{
				command.Reply("Usage: -ns; -ns add <username>");
				return;
			}
			var ns = StatsDatabase.GetNickserv(StatsDatabase.MapUser(command.Sender).Id);
			if (ns == null)
			{
				command.Reply("According to my database, you don't use NickServ. If that's not right, try running -ns update");
			}
			else {
				command.Reply("your NickServ username is " + ns);
			}
		}
	}
}
