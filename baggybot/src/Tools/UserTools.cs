using System;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	internal static class UserTools
	{
		public static bool Validate(ChatUser user)
		{
			Logger.Log(null, "Validating user");
			return user.Client.Operators.Any(op => Validate(user, op));
		}

		private static bool Validate(ChatUser user, Operator op)
		{

			Func<string, string, bool> match = (input, reference) => (reference.Equals("*") || input.Equals(reference));
			var nickM = match(user.Nickname, op.Nick);
			var identM = match(user.UniqueId, op.UniqueId);
			var uidM = true;
			if (op.Uid != "*")
			{
				var dbUser = user.Client.StatsDatabase.MapUser(user);
				uidM = match(dbUser.Id.ToString(), op.Uid);
			}

			return nickM && identM && uidM;
		}
	}
}
