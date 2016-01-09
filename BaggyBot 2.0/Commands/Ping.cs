using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace BaggyBot.Commands
{
	class Ping : ICommand
	{
		public PermissionLevel Permissions { get { return PermissionLevel.All; } }

		private string Colour(int? code)
		{
			//string control = "\\";
			const string control = "\x03";
			const string bold = "\x02";
			if(code.HasValue){
				return control + code.Value + bold;
			}
			return bold + control;
		}

		private string Colourise(double ping){
			if (ping < 10) return Colour(6);
			if (ping < 25) return Colour(9);
			if (ping <= 50) return Colour(3);
			if (ping <= 150) return Colour(8);
			if (ping <= 300) return Colour(7);
			return Colour(4);
		} 

		public void Use(CommandArgs command)
		{
			if (command.Args.Length == 1) {
				var target = command.Args[0];

				var reply = new System.Net.NetworkInformation.Ping().Send(target);

				if (reply.Status == IPStatus.Success) {
					command.ReturnMessage("Reply from {0} in {1}ms{2}", reply.Address.ToString(), Colourise(reply.RoundtripTime) + reply.RoundtripTime, Colour(null));
				} else {
					command.ReturnMessage("Ping failed ({0})", reply.Status);
				}
			} else if (command.Args.Length == 2) {
				var target = command.Args[0];
				var attempts = int.Parse(command.Args[1]);

				var pings = new List<PingReply>();
				long total = 0;
				var successCount = 0;

				for (var i = 0; i < attempts; i++) {
					pings.Add(new System.Net.NetworkInformation.Ping().Send(target));
					if (pings[i].Status == IPStatus.Success) {
						successCount++;
						total += pings[i].RoundtripTime;
                        if (pings[i].RoundtripTime < 500)
                        {
                            Thread.Sleep(500 - (int)pings[i].RoundtripTime);
                        }
					}
				}

				var average = Math.Round(total / (double)successCount, 2);

				var raw = string.Join(", ", pings.Select(reply => (reply.Status == IPStatus.Success ? Colourise(reply.RoundtripTime) + reply.RoundtripTime + "ms" + Colour(null) : Colour(4) + "failed" + Colour(null))));
				var word = successCount == 1 ? "reply" : "replies";
                var address = pings[0].Address == null ? "Unknown IP Address" : pings[0].Address.ToString();
                var number = (double.IsNaN(average) ? "NaN " : average.ToString());
				command.ReturnMessage("{0} {1} from {2}, averaging {3} ({4})", successCount, word, address, Colourise(average) + number + "ms" + Colour(null), raw);

			} else {
				command.ReturnMessage("Pong!");
			}
		}
	}
}
