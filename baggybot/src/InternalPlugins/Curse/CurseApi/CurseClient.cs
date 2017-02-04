using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.InternalPlugins.Curse.CurseApi.Model;
using Newtonsoft.Json;

namespace BaggyBot.InternalPlugins.Curse.CurseApi
{
	class CurseClient
	{
		private readonly CurseApi curseApi = new CurseApi();
		private readonly SocketApi socketApi = new SocketApi();

		private LoginResponse login;
		private WebSocketSession session;

		public event MessageReceivedEvent OnMessageReceived;

		public IReadOnlyList<Group> Groups { get; private set; }
		public IReadOnlyList<Friend> Friends { get; private set; }
		public IReadOnlyDictionary<string, Channel> ChannelMap { get; private set; }
		public IReadOnlyDictionary<string, User> UserMap { get; private set; }

		public void Connect(string username, string password)
		{
			username = WebUtility.UrlEncode(username);
			password = WebUtility.UrlEncode(password);
			login = curseApi.Post<LoginResponse>("https://logins-v1.curseapp.net/login", $"username={username}&password={password}");
			if (login.StatusMessage != null)
			{
				throw new Exception(login.StatusMessage);
			}
			if (login.Status != 1)
			{
				throw new Exception("Unable to connect");
			}
			// Connected successfully; set the API auth token
			curseApi.AuthToken = login.Session.Token;
			ModelExtensions.Api = curseApi;

			GetContacts();
			var mcfStaff = Groups.First(g => g.GroupTitle == "Minecraft Forum").Channels.First(c => c.GroupTitle == "staff-offtopic");
			var messages = mcfStaff.GetMessages(DateTime.MinValue, DateTime.Now, 30);
			// Forward socket events
			socketApi.OnMessageReceived += message => OnMessageReceived?.Invoke(message);

			session = curseApi.Post<WebSocketSession>("https://sessions-v1.curseapp.net/sessions", SessionRequest.Create());
			socketApi.Connect(new Uri(session.NotificationServiceUrl), login.Session.Token);
			socketApi.Login(session.MachineKey, session.SessionID, session.User.UserID);
			socketApi.Listen();
		}

		private void GetContacts()
		{
			var contacts = curseApi.Get<ContactsRequest>("https://contacts-v1.curseapp.net/contacts");
			Groups = contacts.Groups;
			Friends = contacts.Friends;

			ChannelMap = contacts.Groups.SelectMany(g => g.Channels).ToDictionary(c => c.GroupID);
		}

		public void SendMessage(Channel clientChannel, string message)
		{
			var rs = curseApi.Post($"https://conversations-v1.curseapp.net/conversations/{clientChannel.GroupID}", new SendMessageRequest
			{
				Body = message,
				ClientID = session.SessionID,
				MachineKey = session.MachineKey
			});
			;
		}
	}
}
