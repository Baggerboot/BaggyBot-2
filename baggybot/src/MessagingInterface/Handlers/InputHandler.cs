using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaggyBot.Configuration;
using BaggyBot.MessagingInterface.Events;
using BaggyBot.Monitoring;
using IronPython.Runtime.Operations;

namespace BaggyBot.MessagingInterface.Handlers
{
	public class InputHandler : ChatClientEventHandler
	{
		private int height => Console.WindowHeight;
		private int width => Console.WindowWidth;

		private int previousX;
		private int previousY;

		private int currentChannelIndex = 0;

		private string currentText = string.Empty;
		private List<string> textBuffer = new List<string>();

		public InputHandler()
		{
		}

		public override void Initialise()
		{
			Logger.Log(this, "Suppressing console logging...", LogLevel.Info);
			Logger.LogToConsole = false;
		}

		public override void HandleConnectionEstablished()
		{
			Task.Run(() => RenderLoop());
		}

		public override void HandleMessage(MessageEvent ev)
		{
			AddMessage($"[{ev.Message.Channel.Name}] {ev.Message.Sender.Nickname}: {ev.Message.Body}");
			Rerender();
		}

		private void SetCursorPosition(int x, int y)
		{
			previousX = Console.CursorLeft;
			previousY = Console.CursorTop;

			Console.SetCursorPosition(x, y);
		}

		private void RestoreCursorPosition()
		{
			Console.SetCursorPosition(previousX, previousY);
		}

		private void AddMessage(string message)
		{
			textBuffer.Add(message);
		}



		private void ClearCurrentLine()
		{
			var x = Console.CursorLeft;
			var y = Console.CursorTop;
			// Clear the input line by writing spaces to it
			Console.Write(new string(Enumerable.Repeat(' ', width).ToArray()));
			Console.SetCursorPosition(x, y);
		}

		private void ResetTextBuffer()
		{
			for (int i = 0; i < height; i++)
			{
				ClearCurrentLine();
				Console.WriteLine();
			}
		}

		private void Rerender()
		{
			RenderTextBuffer();
			RenderChatArea();
		}

		private void RenderTextBuffer()
		{
			ResetTextBuffer();
			var lastMessages = textBuffer.Skip(textBuffer.Count - height);
			SetCursorPosition(0, 0);
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
			SetCursorPosition(0, height - 1);
			// Clear it
			ClearCurrentLine();
			// Write the input interface
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write($"{channel.Name} > {currentText}");
		}

		private void RenderLoop()
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
						currentChannelIndex = --currentChannelIndex % Client.Channels.Count;
						break;
					case ConsoleKey.Enter:
						var channel = Client.Channels[currentChannelIndex];
						Client.SendMessage(channel, currentText);
						AddMessage($"[{channel.Name}] {Client.Self.Nickname}: {currentText}");
						currentText = string.Empty;
						break;
					case ConsoleKey.Backspace:
						currentText = currentText.Substring(0, currentText.Length - 1);
						break;
					default:
						currentText = currentText + key.KeyChar;
						break;
				}
			}
		}
	}
}