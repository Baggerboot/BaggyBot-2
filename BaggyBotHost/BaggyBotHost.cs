using System;
using System.IO;
using System.Text;
using System.Reflection;

using CsNetLib2;

namespace BaggyBotHost
{
	/// <summary>
	/// This executable will host BaggyBot and CsNetLib separately.
	/// This will allow us to unload BaggBot and IRCSharp, replace their binaries with a newer version,
	/// restart the newer versions, and hook them back up to CsNetLib, while
	/// queueing any messages coming from CsNetLib in the meanwhile.
	/// </summary>
	class BaggyBotHost
	{
		private NetLibClient netClient;
		// We use a dynamic object to make sure that we can instantiate a newer version of BaggyBot and store it in the same field
		private dynamic baggyBot;
		private int domainNumber = 0;

		public BaggyBotHost()
		{
			Console.WriteLine("Starting host application");
			netClient = new NetLibClient(TransferProtocolType.Delimited, Encoding.UTF8);
			baggyBot = new BaggyBot.Bot();
			StartBaggyBot();
			baggyBot.UpdateRequested += new Action<string>(OnUpdateRequested);
		}

		private void OnUpdateRequested(string requestChannel)
		{
			dynamic channels = baggyBot.Detach();
			UpdateBinaries();

			baggyBot.Attach(netClient, channels, requestChannel);
		}

		private void UpdateBinaries()
		{
			Console.WriteLine("Deleting assemblies");	
			File.Delete("BaggyBot20.dll");
			File.Delete("CsNetLib2.dll");
			Console.WriteLine("Moving assemblies");
			File.Move("BaggyBot20.dll.new", "BaggyBot20.dll");
			File.Move("CsNetLib2.dll.new", "CsNetLib2.dll");
		}

		private void StartBaggyBot()
		{
			baggyBot.Connect(netClient);
			baggyBot.UpdateRequested += (Action<string>)OnUpdateRequested;

			bool quitRequested = false;
			while (!quitRequested) {
				lock (baggyBot) 
				{
					quitRequested = baggyBot.QuitRequested;
				}
				System.Threading.Thread.Sleep(1000);
			}
		}

		static void Main(string[] args)
		{
			new BaggyBotHost();
		}
	}
}
