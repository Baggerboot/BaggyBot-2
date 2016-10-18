using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.InternalPlugins.Curse.CurseApi.Model
{
	public class LoginSession
	{
		public int UserID { get; set; }
		public string Username { get; set; }
		public string SessionID { get; set; }
		public string Token { get; set; }
		public string EmailAddress { get; set; }
		public bool EffectivePremiumStatus { get; set; }
		public bool ActualPremiumStatus { get; set; }
		public int SubscriptionToken { get; set; }
		public long Expires { get; set; }
		public long RenewAfter { get; set; }
		public bool IsTemporaryAccount { get; set; }
	}
}
