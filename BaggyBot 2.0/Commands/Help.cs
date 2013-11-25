using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
	class Help : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		public Help(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			ircInterface.SendMessage(command.Channel, GetHelpMessage(command.Args));
		}

		private string GetHelpMessage(string[] args)
		{
			string defaultReply = "Use -help <command> to get help about a specific command. -- Regular commands: help, ns, ping, regen, resolve, version. -- Operator commands: crash, ed, feature, get, join, part, query, say, set, shutdown, snag, sqlreconnect.";

			switch (args.Length) {
				case 1:
					switch (args[0]) {
						case "ed":
							return "Shows detailed information about an exception. Usage: -ed [-ra] [-r] [-i <index>]";
						case "help":
							return "Yo dawg..";
						case "resolve":
							return "Resolves a domain name to its IP addresses. Usage: -resolve <ip>";
						case "ns":
							return "Returns your NickServ username, provided that you have registered a NickServ username, and that you have identified your nickname. Mainly used for debugging purposes, although certain individuals have expressed great pleasure in repeatedly executing this particular command. Usage: -ns";
						case "crash":
							return "Yes, this does exactly what you think it does.";
						case "join":
							return "Join a channel. Usage: -join <channel>";
						case "part":
							return "Leave a channel. Usage: -part <channel>";
						case "nuke":
							return "Clears the database, or a single table. Usage: -nuke [table]";
						case "regen":
							return "Regenerate the graphs on the stats page. Since these aren't automatically generated yet, you have to trigger a regen manually using this command.";
						case "snag":
							return "Snag a quote. If nickname isn't specified, the first received message will be snagged. Usage: -snag [nickname]";
						case "query":
							return "Execute an SQL query. Usage: -query <SQL query>";
						case "shutdown":
							return "Cleanly disconnects and shuts down the bot. Usage: -shutdown";
						case "update":
							return "Updates the bot to a new version, which can optionally be downloaded first. Usage: -update [-d]";
						case "set":
							return "Changes bot settings. Usage: -set <property> [key] <value>";
						case "get":
							return "Retrieves the value for a given property. Usage: -get <property> [key]";
						case "ping":
							return "Returns \"Pong!\" as soon as possible. Useful for debugging or testing your connection.";
						case "feature":
							return "Feature a quote, so it appears in the 'Featured Quote' box on the stats page. Usage: -feature <part of quote contents>";
						case "say":
							return "Make the bot say something. Usage: -say <message>";
						default:
							return defaultReply;
					}
				default:
					return defaultReply;
			}
		}
	}
}
