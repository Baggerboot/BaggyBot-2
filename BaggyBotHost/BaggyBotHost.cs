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

		public BaggyBotHost()
		{
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
				lock (baggyBot) {
					quitRequested = baggyBot.QuitRequested;
				}
				System.Threading.Thread.Sleep(1000);
			}
		}

		private static string EscapeCode(int color)
		{
			return "\x1b[38;5;" + color + "m";
		}

		static void Main(string[] args)
		{
			if (args.Length == 1 && args[0] == "--colortest") {

				Console.WriteLine("Beginning color test");

				int i = 0;

				Console.WriteLine("System colors:");
				for (; i < 16; i++) {
					Console.Write(EscapeCode(i) + "█");
					if (i == 7) {
						Console.WriteLine();
					}
				}
				Console.WriteLine();
				Console.WriteLine("Color cube, 6x6x6:");
				for (int green = 0; green < 6; green++) {
					for (int red = 0; red < 6; red++) {
						for (int blue = 0; blue < 6; blue++) {
							int color = 16 + (red * 36) + (green * 6) + blue;
							Console.Write("{0}█{1:000}█", EscapeCode(color), color);
						}
						Console.Write("\x1b[0m ");
					}
					Console.Write("\n");
				}


				Console.WriteLine();
				Console.WriteLine("Grayscale ramp:");
				for (i = 232; i < 256; i++) {
					Console.Write(EscapeCode(i) + "█");
				}
				Console.WriteLine();
				Console.WriteLine("Done.");

			} else {
				new BaggyBotHost();
			}
		}
	}
}
