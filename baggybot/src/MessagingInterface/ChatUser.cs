using System;
using BaggyBot.Database.Model;

namespace BaggyBot.MessagingInterface
{
	public class ChatUser
	{
		// A users's nickname is their primary handle. 
		// It's generally the name that shows up next to their chat messages,
		// and the name that is used to refer to them. It may or may not be unique.
		public string Nickname { get; }
		// This string is used to uniquely identify a user. Any two ChatUser objects with the same UniqueID
		// will be considered to be the same user. This string can be set to any value considered to be globally
		// unique on the server currently being connected to.
		// Preferably, this unique ID should forever identify the same user, but if this is not possible,
		// it should at least identify the same user until they disconnect.
		public string UniqueId { get; }
		// Should only be set to true if a user's [UniqueId] is 'temporally' unique, in that it will still point
		// to the same user at any given point in the future. Defaults to true.
		public bool HasTemporallyUniqueId { get; }

		// The PreferredName field is a secondary field. It may contain a user's real name, but it may also contain a
		// 'second' nickname. If this field is set, BaggyBot will prefer it over [Nickname] when addressing the user.
		// Therefore, if a user's real name is known, but that user prefers to be addressed by their nickname, this field
		// should /not/ be set to their real name, but remain null.
		public string PreferredName { get; }

		public string AddressableName => PreferredName ?? Nickname;

		public User DbUser { get; private set; }


		public ChatUser(string nickname, string uniqueId, bool hasTemporallyUniqueId = true, string preferredName = null)
		{
			if(nickname == null) throw new ArgumentNullException(nameof(nickname));
			if(uniqueId == null) throw new ArgumentNullException(nameof(uniqueId));

			Nickname = nickname;
			UniqueId = uniqueId;
			HasTemporallyUniqueId = hasTemporallyUniqueId;
			PreferredName = preferredName;
		}

		public void BindDbUser(User user)
		{
			// TODO: Rebinding should probably be possible, since user objects are supposed to be reused.
			// The alternative is not to rebind the user at all, but this means nickname changes may not be handled correctly.
			//if (DbUser == null) throw new InvalidOperationException("User has already been mapped.");
			DbUser = user;
		}

		public override bool Equals(object obj)
		{
			var chatUser = obj as ChatUser;
			if (chatUser?.UniqueId == null || UniqueId == null) return false;
			return string.Equals(UniqueId, chatUser.UniqueId, StringComparison.Ordinal);
		}

		public override int GetHashCode()
		{
			return UniqueId?.GetHashCode() ?? 0;
		}

		public override string ToString()
		{
			return $"{AddressableName} ({UniqueId})";
		}
	}
}
