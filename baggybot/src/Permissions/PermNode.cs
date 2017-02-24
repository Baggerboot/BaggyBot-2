using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Permissions
{
	public class PermNode
	{
		public static PermNode NodeSeparator { get; } = new PermNode(".");

		public static PermNode Baggybot { get; } = new PermNode("baggybot");
		public static PermNode Commands { get; } = new PermNode("baggybot.modules.commands");
		public static PermNode Administration { get; } = new PermNode("baggybot.modules.administration");

		public string Path { get; }

		public PermNode(string path)
		{
			Path = path;
		}

		public PermNode AddNode(string subNode)
		{
			return new PermNode(Path + NodeSeparator.Path + subNode);
		}

		public override string ToString()
		{
			return Path;
		}
	}
}
