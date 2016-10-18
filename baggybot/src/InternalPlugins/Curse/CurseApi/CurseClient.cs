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
		private CurseApi curseApi = new CurseApi();
		private SocketApi socketApi = new SocketApi();
		private string authToken;

		public void Connect(string username, string password)
		{
			username = WebUtility.UrlEncode(username);
			password = WebUtility.UrlEncode(password);
			var login = curseApi.Post<LoginResponse>("https://logins-v1.curseapp.net/login", $"username={username}&password={password}");

			if (login.StatusMessage != null)
			{
				throw new Exception(login.StatusMessage);
			}
			if (login.Status != 1)
			{
				throw new Exception("Unable to connect");
			}
			curseApi.AuthToken = login.Session.Token;

			var socketSession = curseApi.Post<WebSocketSession>("https://sessions-v1.curseapp.net/sessions", SessionRequest.Create());
			socketApi.Connect(new Uri(socketSession.NotificationServiceUrl), login.Session.Token);
			socketApi.Login(socketSession.MachineKey, socketSession.SessionID, socketSession.User.UserID);
		}
	}
}
