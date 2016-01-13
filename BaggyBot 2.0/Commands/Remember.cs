using System.Text.RegularExpressions;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
	class Remember : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }
		private readonly DataFunctionSet dataFunctionSet;

		public Remember(DataFunctionSet df)
		{
			dataFunctionSet = df;
		}

		public void Use(CommandArgs command)
		{
			var key = command.Args[0];
			var value = command.FullArgument.Substring(key.Length);
			if (value != string.Empty)
			{
				value = value.Substring(1);
			}
			if (value.StartsWith("$"))
			{

				command.ReturnMessage("You sneaky bastard, you didn't think I was going to allow this, did you?");
				return;
			}
			if (value == string.Empty)
			{
				command.Reply("Usage: -rem <trigger> <response> - Example: -rem pong Ping!");
				return;
			}
			dataFunctionSet.UpsertMiscData("rem", key, value);
			command.Reply("Saved.");
		}
		public void UseRem(CommandArgs command)
		{
			string format;
			if (dataFunctionSet.MiscDataContainsKey("rem", command.Command))
			{
				format = dataFunctionSet.GetMiscData("rem", command.Command);
			}
			else
			{
				return;
			}

			var args = new object[command.Args.Length];
			for (var i = 0; i < command.Args.Length; i++)
			{
				args[i] = command.Args[i];
			}

			var currentIndex = -1;
			var openIndex = -1;
			for (var i = 0; i < format.Length; i++)
			{
				if (format[i] == '{')
				{
					openIndex = i;
				}
				else if (format[i] == '}' && openIndex > 0)
				{
					currentIndex++;
					if (format[openIndex - 1] == ')')
					{
						for (var j = openIndex - 2; j >= 0; j--)
						{
							if (format[j] == '(')
							{
								var type = format.Substring(j + 1, openIndex - j - 2);
								switch (type)
								{
									case "int":
										var value = int.Parse(command.Args[currentIndex]);
										args[currentIndex] = value;
										break;
								}
							}
						}
					}
					openIndex = -1;
				}
			}
			var rgx = new Regex(@"\(.*?\)\{");
			format = rgx.Replace(format, match => "{");
			var res = string.Format(format, args);
			command.ReturnMessage(res);
		}
	}
}
