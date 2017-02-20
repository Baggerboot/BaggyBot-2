using System.Linq;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	internal class Regenerate : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Name => "regen";
		public override string Usage => "";
		public override string Description => "Regenerates statistics from the chat log.";
		
		public override void Use(CommandArgs command)
		{
			var messages = StatsDatabase.GetMessages();

			var usage = messages.SelectMany(m => WordTools.GetWords(m.Message)).GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());

			StatsDatabase.ResetWords(usage);
			command.Reply($"Regenerated word count for {usage.Count} words (Total: {usage.Sum(p => p.Value)}).");
		}
	}
}
