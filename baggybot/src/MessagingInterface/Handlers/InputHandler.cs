using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Monitoring;
using IronPython.Runtime.Operations;

namespace BaggyBot.MessagingInterface.Handlers
{
	public class InputHandler : ChatClientEventHandler
	{
		private int Height => Console.WindowHeight;
		private int Width => Console.WindowWidth;

		private const int ChatAreaHeight = 1;
		private int TextBufferHeight => Height - ChatAreaHeight;

		private int currentChannelIndex = 0;
		private ChatChannel CurrentChannel => Client.Channels[currentChannelIndex];

		private string currentText = string.Empty;
		private object consoleLock = new object();

		private Dictionary<string, List<string>> textBuffers = new Dictionary<string, List<string>>();

		public InputHandler()
		{
		}

		public override void Initialise()
		{
			Logger.Log(this, "Suppressing console logging...", LogLevel.Info);
			Logger.LogToConsole = false;
			Console.CursorVisible = false;
		}

		public override void HandleConnectionEstablished()
		{
			Task.Run(() => InputLoop());
			Task.Run(() => RenderLoop());
		}

		public override void HandleMessage(MessageEvent ev)
		{
			AddMessage(ev.Message.Channel.Identifier, FormatMessage(ev.Message));
			Rerender();
		}

		private string FormatMessage(ChatMessage message)
		{
			return FormatMessage(message.Channel.Name, message.Sender.Nickname, message.Body);
		}

		private string FormatMessage(string channel, string sender, string body)
		{
			return $"[{channel}] {sender}: {body}";
		}

		private void AddMessage(string bufferName, string message)
		{
			if (!textBuffers.ContainsKey(bufferName)) InitBuffer(bufferName);
			textBuffers[bufferName].Add(message);
		}

		private void InitBuffer(string bufferName)
		{
			textBuffers[bufferName] = new List<string>();
			Task.Run(() =>
			{
				IEnumerable<ChatMessage> backlog;
				try
				{
					backlog = Client.GetBacklog(Client.GetChannel(bufferName), DateTime.Now, DateTime.MinValue).Reverse().Take(Height);

				}
				catch (Exception)
				{
					return;
				}
				foreach (var line in backlog)
				{
					textBuffers[bufferName].Add(FormatMessage(line));
				}
				Rerender();
			});
		}

		private void ClearCurrentLine(char clearChar = ' ')
		{
			var x = Console.CursorLeft;
			var y = Console.CursorTop;
			// Clear the input line by writing spaces to it
			Console.Write(new string(Enumerable.Repeat(clearChar, Width).ToArray()));
			Console.SetCursorPosition(x, y);
		}

		private void ResetTextBuffer()
		{
			for (var i = 0; i < Height; i++)
			{
				ClearCurrentLine();
				Console.WriteLine();
			}
		}

		private void Rerender()
		{
			lock (consoleLock)
			{
				RenderTextBuffer(CurrentChannel.Identifier);
				RenderChatArea();
			}
		}

		private void RenderTextBuffer(string bufferName)
		{
			ResetTextBuffer();
			if (!textBuffers.ContainsKey(bufferName)) InitBuffer(bufferName);

			var currentBuffer = textBuffers[bufferName];
			var lastMessages = currentBuffer.Skip(currentBuffer.Count - Height);
			Console.SetCursorPosition(0, 0);
			Console.ForegroundColor = ConsoleColor.White;
			foreach (var message in lastMessages)
			{
				Console.WriteLine(message);
			}
		}

		private void RenderChatArea()
		{
			var channel = Client.Channels[currentChannelIndex];

			// Move the cursor to the input line
			Console.SetCursorPosition(0, TextBufferHeight);
			// Clear it
			ClearCurrentLine();
			// Write the input interface
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write($"{channel.Name} > {currentText}");
		}

		private void InputLoop()
		{
			while (Client.Connected)
			{
				Rerender();
				var key = Console.ReadKey(true);
				switch (key.Key)
				{
					case ConsoleKey.UpArrow:
						currentChannelIndex = ++currentChannelIndex % Client.Channels.Count;
						break;
					case ConsoleKey.DownArrow:
						currentChannelIndex = (currentChannelIndex + Client.Channels.Count - 1) % Client.Channels.Count;
						break;
					case ConsoleKey.Enter:
						Client.SendMessage(CurrentChannel, currentText);
						AddMessage(CurrentChannel.Identifier, FormatMessage(CurrentChannel.Name, Client.Self.Nickname, currentText));
						currentText = string.Empty;
						break;
					case ConsoleKey.Backspace:
						if (currentText.Length > 0)
						{
							currentText = currentText.Substring(0, currentText.Length - 1);
						}
						break;
					default:
						currentText = currentText + key.KeyChar;
						break;
				}
			}
		}

		private void RenderLoop()
		{
			var show = false;
			while (Client.Connected)
			{
				lock (consoleLock)
				{
					show = !show;
					Console.Write(show ? "_" : " ");
					Console.CursorLeft = Console.CursorLeft-1;
				}
				Thread.Sleep(500);
			}
		}
	}
}