﻿using BaggyBot.MessagingInterface.Events;

namespace BaggyBot.MessagingInterface.Handlers.Administration.Triggers
{
	public class SimilarityTrigger
	{
		public string Percentage { get; set; }

		public UserSimilarityTrigger Create()
		{
			return new UserSimilarityTrigger
			{
				Percentage = Percentage
			};
		}
	}

	public class UserSimilarityTrigger : SimilarityTrigger, ITriggerable
	{
		private MessageEvent previous;

		public bool Check(MessageEvent ev)
		{
			// TODO: implement message similarity

			var similar = ev.Message.Body.ToLower() == previous?.Message?.Body?.ToLower();
			previous = ev;
			return similar;
		}

		public void Initialise()
		{
			
		}
	}
}