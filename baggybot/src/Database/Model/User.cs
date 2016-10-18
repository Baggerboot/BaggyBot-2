using LinqToDB.Mapping;

namespace BaggyBot.Database.Model
{
	[Table(Name = "irc_user")]
	public class User : Poco
	{
		// The internal UserId of this user.
		[Column(Name = "id"), PrimaryKey, Identity]
		public int Id { get; set; }

		// If the chat server/plugin is able to assign 'temporally' unique IDs to users, this field should be set to a user's unique ID.
		// If the server is unable to do this, this field should remain null.
		[Column(Name = "unique_id"), Nullable]
		public string UniqueId { get; set; }

		// Holds the first known nickname for this user. This value should never be changed, and it is not used internally.
		// It only exists to aid in database administration/management by allowing the DB administrator to lookup users by a nickname that's guaranteed not to change.
		[Column(Name = "original_nickname"), NotNull]
		public string OriginalNickname { get; set; }

		// Holds the nickname currently used by the user. If the user changes their nickname, this value should be updated accordingly.
		[Column(Name = "nickname"), NotNull]
		public string Nickname { get; set; }

		// Holds the name the user currently prefers to be addressed by, according to the chat server/plugin.
		// If the user's preference changes, this value should be updated accordingly.
		[Column(Name = "addressable_name"), NotNull]
		public string AddressableName { get; set; }

		// Username override. Should normally be Null. If set, the value of AddressableName will be ignored,
		// and the user will be addressed by the name specified in this column instead.
		[Column(Name = "addressable_name_override"), Nullable]
		public string AddressableNameOverride { get; set; }
	}
}
