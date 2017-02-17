using System;
using System.Threading.Tasks;
using BaggyBot.MessagingInterface.Events;

namespace BaggyBot.MessagingInterface.Handlers.Administration.Triggers
{
	public class RepetitionTrigger
	{
		public bool Reset { get; set; }
		public int Threshold { get; set; }
		public string TickDown { get; set; }

		public UserRepetitionTrigger Create()
		{
			return new UserRepetitionTrigger
			{
				Reset = Reset,
				Threshold = Threshold,
				TickDown = double.Parse(TickDown)
			};
		}
	}

	public class UserRepetitionTrigger : RepetitionTrigger, ITriggerable
	{
		private int counter;
		public new double TickDown { get; set; }

		public bool Check(MessageEvent ev)
		{
			Task.Run(() =>
			{
				Task.Delay(TimeSpan.FromSeconds(TickDown)).Wait();
				lock (this)
				{
					counter--;
				}
			});
			lock (this)
			{
				counter++;
				if (counter >= Threshold && Reset)
				{
					counter = 0;
					return true;
				}
				return counter >= Threshold;
			}
		}

		public void Initialise()
		{
		}
	}
}