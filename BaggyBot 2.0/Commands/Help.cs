using System.Collections.Generic;
using System.Linq;

namespace BaggyBot.Commands
{
	internal class Help : ICommand
	{
		public PermissionLevel Permissions => PermissionLevel.All;
		public string Usage => "<command>";
		public string Description => "Get help about my commands.";

		private readonly Dictionary<string, ICommand> commandList; 

		public Help(Dictionary<string, ICommand> commandList)
		{
			this.commandList = commandList;
		}

		public void Use(CommandArgs command)
		{


			if (command.Args.Length == 0)
			{
				var availableCommands = string.Join(", ", commandList.Select(pair => pair.Key));
				command.ReturnMessage($"Use -help <command> to get help about a specific command. -- Available commands: {availableCommands}");
			}
			else if (command.Args.Length == 1)
			{
				if (commandList.ContainsKey(command.Args[0]))
				{
					var cmd = commandList[command.Args[0]];
					command.Reply($"{command.Args[0]}: {cmd.Description} (usable by {cmd.Permissions}) -- Usage: \x02{command.Args[0]} {cmd.Usage}\x0F");
				}
			}
			else
			{

			}
		}
	}
}
