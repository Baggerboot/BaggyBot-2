using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands
{
    class Alias : ICommand
    {
        public Dictionary<string, string> Aliases { get; private set; }

        public PermissionLevel Permissions { get { return PermissionLevel.All; } }

        public Alias()
        {
            Aliases = new Dictionary<string, string>();
        }

        public void Use(CommandArgs command)
        {
            if (command.Args.Length > 2)
            {
                var key = command.Args[0];
                var value = string.Join(" ", command.Args.Skip(1));
                if (value.StartsWith("-"))
                {
                    value = value.Substring(1);
                }
                Aliases[key] = value;
                command.Reply("I've aliased {0} to \"{1}\"", key, value);
               /* if (Aliases.ContainsKey(key))
                {
                    Aliases[key] = value;
                }
                else
                {
                    Aliases.Add(key, value);
                }*/
                
            }
            else
            {
                command.Reply("usage: -alias <key> <command> [parameters ...]");
            }
        }
    }
}
