using System;
using IRCSharp;
using BaggyBot.DataProcessors;
using IRCSharp.IRC;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	public static class UserTools
	{
		public static DataFunctionSet DataFunctionSet
		{
			set;
			private get; // restrict access to the DataFunctionSet class to only this class
		}

		/// <summary>
		/// Checks whether the specified user has operator permissions
		/// </summary>
		/// <returns></returns>
		delegate bool Match(string input, string reference);
		public static bool Validate(IrcUser user)
		{
			Logger.Log(null, "Validating user");
			Match match = (input, reference) => {
				return (reference.Equals("*") || input.Equals(reference));
			};

			var nick = Settings.Instance["operator_nick"];
			var ident = Settings.Instance["operator_ident"];
			var host = Settings.Instance["operator_host"];
			var uid = Settings.Instance["operator_uid"];
			var uids = DataFunctionSet.GetUids(user);

			if (uids.Length > 1) {
				Logger.Log(null, String.Format("Failed to validate {0} ({1}, {2}); GetUids() returned more than one user ID.", user.Nick, user.Ident, user.Hostmask), LogLevel.Warning);
				return false;
			}
			if(uids.Length == 0){
				Logger.Log(null, String.Format("Failed to validate {0} ({1}, {2}); GetUids() returned no user IDs.", user.Nick, user.Ident, user.Hostmask), LogLevel.Warning);
				return false;
			}
			var nickM = match(user.Nick, nick);
			var identM = match(user.Ident, ident);
			var hostM = match(user.Hostmask, host);
			var uidM = match(uids[0].ToString(), uid);

			return (nickM && identM && hostM && uidM);
		}
	}
}
