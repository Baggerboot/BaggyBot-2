
using System;
using BaggyBot.Plugins;

namespace BaggyBot.MessagingInterface
{
	public class ChatUser
	{
		// The chat client (chat plugin) this user belongs to.
		public Plugin Client { get; }

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

		// The name field is a secondary field. It may contain a user's real name, but it may also contain a
		// 'second' nickname. If this field is set, BaggyBot will prefer it over [Nickname] when addressing the user.
		// Therefore, if a user's real name is known, but that user prefers to be addressed by their nickname, this field
		// should /not/ be set to their real name, but remain null.
		public string Name { get; }

		public string AddressableName => Name ?? Nickname;


		public ChatUser(Plugin client, string nickname, string uniqueId, bool hasTemporallyUniqueId = true, string name = null)
		{
			if(nickname == null) throw new ArgumentNullException(nameof(nickname));
			if(uniqueId == null) throw new ArgumentNullException(nameof(uniqueId));

			Client = client;
			Nickname = nickname;
			UniqueId = uniqueId;
			HasTemporallyUniqueId = hasTemporallyUniqueId;
			Name = name;
		}

		public override string ToString()
		{
			return $"{AddressableName} ({UniqueId})";
		}
	}
}
