using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IRCSharp;

namespace BaggyBot.Tools
{
	/// <summary>
	/// For all Tools classes goes: they must be static, and they may not change any state.
	/// </summary>
	internal static class UserTools
	{
		private static Settings settings = Settings.Instance;

		internal static DataFunctionSet DataFunctionSet
		{
			set;
			private get; // restrict access to the DataFunctionSet class to only this class
		}

		/// <summary>
		/// Checks whether the specified user has operator permissions
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		delegate bool match(string input, string reference);
		internal static bool Validate(IrcUser user)
		{
			match match = (input, reference) => {
				return (reference.Equals("*") || input.Equals(reference));
			};

			string nick = settings["operator_nick"];
			string ident = settings["operator_ident"];
			string host = settings["operator_host"];
			string uid = settings["operator_uid"];

			int[] uids = DataFunctionSet.GetUids(user);
			if (uids.Length > 1) {
				Logger.Log(String.Format("Failed to validate {0} ({1}, {2}); GetUids() returned more than one user ID.", user.Nick, user.Ident, user.Hostmask), LogLevel.Warning);
				return false;
			} else if(uids.Length == 0){
				Logger.Log(String.Format("Failed to validate {0} ({1}, {2}); GetUids() returned no user IDs.", user.Nick, user.Ident, user.Hostmask), LogLevel.Warning);
				return false;
			}

			return (match(nick, user.Nick) && match(ident, user.Ident) && match(host, user.Hostmask) && match(uids[0].ToString(), uid));
		}
	}
}
