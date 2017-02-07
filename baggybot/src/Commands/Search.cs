using System.Linq;
using BaggyBot.CommandParsing;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	internal class Search : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.All;
		public override string Name => "search";
		public override string Usage => "[-n|--max-results] <search query>";
		public override string Description => "Search the IRC backlog for a message. Limits the number of displayed results to 1 by default. Use -n (--max-results) to display more results.";
		
		public override void Use(CommandArgs command)
		{
			if(command.FullArgument == null)
			{
				InformUsage(command);
				return;
			}

			var parser = new CommandParser(new Operation().AddKey("max-results", "1", 'n').AddRestArgument());

			var result = parser.Parse(command.FullArgument);
			var numDisplayed = result.GetKey<int>("max-results");
			if (numDisplayed > 3 && !Client.Validate(command.Sender))
			{
				command.Reply("only bot operators may request more than three results.");
				return;
			}
			var query = result.RestArgument;

			var matches = StatsDatabase.FindLine(query);
			switch (matches.Count)
			{
				case 0:
					command.Reply("no matches found.");
					break;
				case 1:
					command.Reply("1 match found: " + matches[0]);
					break;
				default:
					if (numDisplayed == 1)
						command.Reply($"{matches.Count} matches (1 displayed): {matches[0]}");
					else
					{
						int matchNumber = 0;
						foreach (var match in matches.Take(numDisplayed))
						{
							command.Reply($"match {++matchNumber} of {numDisplayed}: {match}");
						}
					}
					break;
			}
		}
	}
}
