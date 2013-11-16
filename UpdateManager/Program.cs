using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace UpdateManager
{
	class Program
	{

		public Program(string argument)
		{
			Run(argument);
		}
		private void Run(string argument)
		{
			Thread.Sleep(2000);
			BinaryFormatter bf = new BinaryFormatter();
			Object input;

			Console.WriteLine("Waiting for file to become unlocked...");

			using (FileStream fs = new FileStream("socket.stream", FileMode.Open, FileAccess.ReadWrite, FileShare.None, 100)) {
				input = bf.Deserialize(fs);
			}

			Console.WriteLine("Done. Deserializing socket...");

			SocketInformation si = (SocketInformation)input;

			// This kills the bot
			Socket s = new Socket(si);

			//Socket s = (Socket)input;

			TcpClient client = new TcpClient();
			client.Client = s;

			bool recover = argument == "-recover";

			if (recover) {
				Console.WriteLine("Recovering bot...");
				Thread.Sleep(1000);
			} else {
				Console.WriteLine("Updating bot...");
				UpdateBot();
			}

			Process proc = new Process();
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardInput = true;
			proc.StartInfo.FileName = "BaggyBot20.exe";
			proc.StartInfo.Arguments = recover ? "-ds" : ( "-ds -pv " + argument );
			proc.Start();

			si = s.DuplicateAndClose(proc.Id);
			bf.Serialize(proc.StandardInput.BaseStream, si);
		}

		private void UpdateBot()
		{
			Thread.Sleep(1000);
			if (File.Exists("CSNetLib.dll")) {
				System.IO.File.Delete("CSNetLib.dll");
				System.IO.File.Delete("IRCSharp.dll");
				System.IO.File.Move("CSNetLib_new.dll", "CSNetLib.dll");
				System.IO.File.Move("IRCSharp_new.dll", "IRCSharp.dll");
			}
			System.IO.File.Delete("BaggyBot20.exe");

			System.IO.File.Move("BaggyBot20_new.exe", "BaggyBot20.exe");

		}

		static void Main(string[] args)
		{
			new Program(args[0]);
		}
	}
}
