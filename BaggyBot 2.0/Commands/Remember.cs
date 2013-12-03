using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace BaggyBot.Commands
{
	class Remember : ICommand
	{
		private IrcInterface ircInterface;
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private Dictionary<string, string> RemList = new Dictionary<string, string>();

		public Remember(IrcInterface inter)
		{
			ircInterface = inter;
		}

		public void Use(CommandArgs command)
		{
			var key = command.Args[0];
			if (key.StartsWith("$")) {
				ircInterface.SendMessage(command.Channel, "You sneaky bastard, you didn't think I was going to allow this, did you?");
				return;
			}
			var format = command.FullArgument.Substring(key.Length);
			if (format == string.Empty) {
				ircInterface.SendMessage(command.Channel, "Usage: -rem <trigger> <response> - Example: -rem hex Hexadecimal value of {0} is (int){0:X8}");
				return;
			}
			RemList.Add(key, format);
			ircInterface.SendMessage(command.Channel, "Saved.");
		}
		public void UseRem(CommandArgs command)
		{
			string format;
			if (RemList.ContainsKey(command.Command)) {
				format = RemList[command.Command];
			} else {
				return;
			}

			object[] args = new object[command.Args.Length];
			for (int i = 0; i < command.Args.Length; i++) {
				args[i] = command.Args[i];
			}

			int currentIndex = -1;
			int openIndex = -1;
			for (int i = 0; i < format.Length; i++) {
				if (format[i] == '{') {
					openIndex = i;
				}else if(format[i] == '}' && openIndex > 0){
					currentIndex++;
					if (format[openIndex - 1] == ')') {
						for (int j = openIndex - 2; j >= 0; j--) {
							if (format[j] == '(') {
								var type = format.Substring(j + 1, openIndex - j -2);
								switch (type) {
									case "int":
										int value = int.Parse(command.Args[currentIndex]);
										args[currentIndex] = value;
										break;
								}
							}
						}
					}
					openIndex = -1;
				}
			}
			Regex rgx = new Regex(@"\(.*?\)\{");
			format = rgx.Replace(format, new MatchEvaluator((match) => {
				return "{";
			}));
			var res = string.Format(format, args);
			ircInterface.SendMessage(command.Channel, res);
		}
	}
}
