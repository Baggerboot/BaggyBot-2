using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.InternalPlugins.Curse.CurseApi;
using BaggyBot.InternalPlugins.Curse.CurseApi.Model;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;
using Mono.CSharp.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaggyBot.InternalPlugins.Curse
{
	class CursePlugin : Plugin
	{
		public override event DebugLogEvent OnDebugLog;
		public override event MessageReceivedEvent OnMessageReceived;
		public override event NameChangeEvent OnNameChange;
		public override event KickEvent OnKick;
		public override event KickedEvent OnKicked;
		public override event ConnectionLostEvent OnConnectionLost;
		public override event QuitEvent OnQuit;
		public override event JoinChannelEvent OnJoinChannel;
		public override event PartChannelEvent OnPartChannel;

		public override string ServerType => "curse";

		public override IReadOnlyList<ChatChannel> Channels { get; }

		public override bool Connected { get; }

		private CurseClient client = new CurseClient();
		private NetworkCredential loginCredentials;

		public CursePlugin(ServerCfg config) : base(config)
		{
			loginCredentials = new NetworkCredential(config.Identity.Nick, config.Password);
		}

		public override MessageSendResult SendMessage(ChatChannel target, string message)
		{
			throw new NotImplementedException();
		}

		public override bool JoinChannel(ChatChannel channel)
		{
			throw new NotImplementedException();
		}

		public override void Part(ChatChannel channel, string reason = null)
		{
			throw new NotImplementedException();
		}

		public override void Quit(string reason)
		{
			throw new NotImplementedException();
		}

		public override bool Connect()
		{
			client.Connect(loginCredentials.UserName, loginCredentials.Password);
			return true;
		}

		public override void Disconnect()
		{
			throw new NotImplementedException();
		}

		public override void Dispose()
		{
			throw new NotImplementedException();
		}

		public override ChatUser FindUser(string name)
		{
			throw new NotImplementedException();
		}


	}
}