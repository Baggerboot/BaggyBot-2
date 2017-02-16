using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface.Events;
using IronPython.Modules;

namespace BaggyBot.MessagingInterface.Handlers.Administration
{
	internal class AdministrationHandler : ChatClientEventHandler
	{
		private readonly Dictionary<string, UserEvent[]> userEventMapping = new Dictionary<string, UserEvent[]>();

		public override void HandleMessage(MessageEvent ev)
		{
			var cfg = ConfigManager.Config.Administration;

			if (cfg.Enabled)
			{
				CreateMapping(ev);

				foreach (var handler in userEventMapping[ev.Message.Sender.UniqueId])
				{
					if (handler.Triggers.Any(t => t.ShouldTrigger(ev)))
					{
						var actions = handler.GetActions(ev);
						foreach (var action in actions)
						{
							switch (action)
							{
								case Action.Warn:
									WarnUser(ev, handler.Messages.Warn);
									break;
								case Action.WarnKick:
									WarnUser(ev, handler.Messages.WarnKick);
									break;
								case Action.WarnBan:
									WarnUser(ev, handler.Messages.WarnBan);
									break;
								case Action.Delete:
									Client.Delete(ev.Message);
									break;
								case Action.Kick:
									Client.Kick(ev.Message.Sender);
									break;
								case Action.Ban:
									Client.Ban(ev.Message.Sender);
									break;
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
					}
				}
			}
		}

		private void WarnUser(MessageEvent ev, string message)
		{
			if (Client.Capabilities.AtMention)
			{
				message = message.Replace("$user", "@" + ev.Message.Sender.AddressableName);
			}
			else
			{
				message = message.Replace("$user", ev.Message.Sender.AddressableName);
			}
			ev.ReturnMessageCallback(message);
		}

		private void CreateMapping(MessageEvent ev)
		{
			if (userEventMapping.ContainsKey(ev.Message.Sender.UniqueId)) return;

			userEventMapping.Add(ev.Message.Sender.UniqueId, 
				ConfigManager.Config.Administration.Events
				.Where(e => e.Enabled)
				.Select(e => e.Create()).ToArray());
		}
	}
}