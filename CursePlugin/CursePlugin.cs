using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Plugins;

using CefSharp;
using CefSharp.OffScreen;
using IRCSharp.IRC;


namespace CursePlugin
{

	public class CursePlugin :BaggyBot.Plugins.Plugin
	{
		public override string ServerType => "curse";

		private static ChromiumWebBrowser browser;

		public override event DebugLogEvent OnDebugLog;
		public override event MessageReceivedEvent OnMessageReceived;
		public override event NameChangeEvent OnNameChange;
		public override event KickEvent OnKick;
		public override event KickedEvent OnKicked;
		public override event QuitEvent OnQuit;
		public override event JoinChannelEvent OnJoinChannel;
		public override event PartChannelEvent OnPartChannel;
		public override event ConnectionLostEvent OnConnectionLost;

		public override IReadOnlyList<ChatChannel> Channels { get; }
		public override bool Connected { get; }
		

		public CursePlugin(ServerCfg cfg) : base(cfg)
		{
			var settings = new CefSettings
			{
				RemoteDebuggingPort = 8081,
			};
			
			System.IO.Directory.SetCurrentDirectory(System.IO.Directory.GetCurrentDirectory() + "\\" + "plugins");

			Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

			var curseUrl = "https://www.curse.com/minecraftforum/staff-offtopic";
			//var curseUrl = "https://www.curse.com/servers/Mf63Aa/staff-offtopic";

			Cef.GetGlobalCookieManager().SetCookieAsync(curseUrl, new Cookie
			{
				Name = "Auth.Token",
				Creation = DateTime.Now,
				Domain = ".curse.com",
				Expires = DateTime.Now.AddYears(1),
				HttpOnly = true,
				LastAccess = DateTime.Now,
				Path = "/",
				Secure = true,
				Value = cfg.Password
			}).Wait();

			browser = new ChromiumWebBrowser(curseUrl);
			var firstLoad = true;
			browser.LoadingStateChanged += (sender, args) =>
			{
				if (!args.IsLoading)
				{
					//var jq = browser.EvaluateScriptAsync(JQuery.SourceCode).Result;
					if (firstLoad)
					{
						firstLoad = false;
						// Welcome screen, launch the webapp
						//browser.ExecuteScriptAsync($"document.cookie = 'Auth.Token={cfg.Password}'");
						browser.EvaluateScriptAsync("document.getElementsByClassName('app-launch-button')[0].click()").Wait();
						browser.EvaluateScriptAsync("document.getElementsByClassName('launch-tabs')[0].children[1].click()").Wait();
						browser.ExecuteScriptAsync("document.");
					}
					else if (browser.Address == "https://www.curse.com/home")
					{
						// Need to go to the server
					}
					else
					{
						
					}
				}
			};

			new Thread(() =>
			{
				while (true)
				{
					var bmp = browser.ScreenshotOrNull();
					bmp?.Save("screenshot.png");
					Thread.Sleep(500);

					
				}

			}).Start();

		}

		public override ChatUser FindUser(string name)
		{
			throw new NotImplementedException();
		}

		public override void Disconnect()
		{
			Cef.Shutdown();
		}

		public override void Dispose()
		{
		}

		public override bool Connect()
		{
			return true;
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
	}

	public class CurseKeyboardHandler : IKeyboardHandler
	{
		public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode,
			CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
		{
			throw new NotImplementedException();
		}

		public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode,
			CefEventFlags modifiers, bool isSystemKey)
		{
			throw new NotImplementedException();
		}
	}
}
