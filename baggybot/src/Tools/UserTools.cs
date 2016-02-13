using System;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.DataProcessors;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	internal static class UserTools
	{
		public static DataFunctionSet DataFunctionSet
		{
			private get;
			set;
		}

		/// <summary>
		/// Checks whether the specified user has operator permissions
		/// </summary>
		private delegate bool Match(string input, string reference);
		public static bool Validate(IrcUser user)
		{
			Logger.Log(null, "Validating user");
			Match match = (input, reference) => (reference.Equals("*") || input.Equals(reference));

			// TODO: allow validation of multiple operators
			var op = ConfigManager.Config.Operators.First();

			var nick = op.Nick;
			var ident = op.Ident;
			var host = op.Host;
			var uid = op.Uid;
			int[] uids;
			try
			{
				uids = DataFunctionSet.GetUids(user);
			}
			catch (Exception e)
			{
				Logger.Log(null, $"Failed to get UID for {user.Nick} ({user.Ident}, {user.Hostmask}); An exception occurred while trying to query the database: {e.GetType()}: \"{e.Message}\"", LogLevel.Warning);
				uids = new[] { -1 };
			}

			if (uids.Length > 1)
			{
				Logger.Log(null, $"Failed to validate {user.Nick} ({user.Ident}, {user.Hostmask}); GetUids() returned more than one user ID.", LogLevel.Warning);
				return false;
			}
			if (uids.Length == 0)
			{
				Logger.Log(null, $"Failed to validate {user.Nick} ({user.Ident}, {user.Hostmask}); GetUids() returned no user IDs.", LogLevel.Warning);
				return false;
			}
			var nickM = match(user.Nick, nick);
			var identM = match(user.Ident, ident);
			var hostM = match(user.Hostmask, host);
			var uidM = match(uids[0].ToString(), uid);

			return nickM && identM && hostM && uidM;
		}
	}
}
