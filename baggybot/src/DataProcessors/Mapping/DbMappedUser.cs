using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface;

namespace BaggyBot.DataProcessors.Mapping
{
	class DbMappedUser : ChatUser
	{
		public Database.Model.User DbUser { get; }

		private DbMappedUser(Database.Model.User dbUser, ChatUser chatUser) : base(chatUser.Client, chatUser.Nickname, chatUser.UniqueId, chatUser.HasTemporallyUniqueId, chatUser.Name)
		{
			DbUser = dbUser;
		}

		public static DbMappedUser Create(Database.Model.User dbUser, ChatUser chatUser)
		{
			return new DbMappedUser(dbUser, chatUser);
		}
	}
}
