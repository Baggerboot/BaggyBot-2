using System;
using System.Collections.Generic;
using System.Linq;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Monitoring;
using BaggyBot.Permissions;

namespace BaggyBot.MessagingInterface.Handlers.Administration
{
	internal class AdministrationHandler : ChatClientEventHandler
	{
		private readonly Dictionary<string, UserEvent[]> userEventMapping = new Dictionary<string, UserEvent[]>();

		public override void Initialise()
		{
			if (ConfigManager.Config.Administration.Enabled)
			{
				var evts = Client.Configuration.AdministrationEvents.Where(e => e.Enabled);
				Logger.Log(this, $"Administration module enabled: {evts.Count()} events configured.", LogLevel.Info);
			}
		}

		public override void HandleMessage(MessageEvent ev)
		{
			// Only handle events if the administration module is enabled globally
			if (!ConfigManager.Config.Administration.Enabled) return;
			// Only handle events for non-operators and users who aren't ignored
			if (Client.Permissions.Test(ev.Message, PermNode.Administration.AddNode("ignore-user"))) return;

			foreach (var handler in GetMapping(ev.Message.Sender.UniqueId))
			{
				if (!handler.Triggers.Any(t => t.ShouldTrigger(ev))) continue;
				Logger.Log(this, $"Event \"{handler.Name}\" triggered", LogLevel.Trace);
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
							Client.Kick(ev.Message.Sender, ev.Message.Channel);
							break;
						case Action.Ban:
							Client.Ban(ev.Message.Sender, ev.Message.Channel);
							break;
						default:
							throw new ArgumentOutOfRangeException();
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
			ev.ReturnMessage(message);
		}

		private IEnumerable<UserEvent> GetMapping(string uniqueId)
		{
			if (userEventMapping.ContainsKey(uniqueId)) return userEventMapping[uniqueId];

			var adminEvents = Client.Configuration.AdministrationEvents.Where(e => e.Enabled);
			userEventMapping.Add(uniqueId, adminEvents.Select(e => e.Create()).ToArray());
			return userEventMapping[uniqueId];
		}
	}
}