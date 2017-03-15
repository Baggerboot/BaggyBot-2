using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using BaggyBot.Configuration;

namespace BaggyBot.Monitoring.Diagnostics
{
	/// <summary>
	/// This class gathers data about the bot's performance and sends it to a logger so it can be written to a log file.
	/// </summary>
	internal class BotDiagnostics : IDisposable
	{
		private const string PerfLogFile = "performance_log.csv";
		private Timer taskScheduler;
		private PerformanceCounter pc;
		private readonly PerformanceLogger performanceLogger;

		public BotDiagnostics(Action<string> notifyCallback)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, args) => HandleException(args, notifyCallback);
			if (ConfigManager.Config.LogPerformance)
			{
				performanceLogger = new PerformanceLogger(PerfLogFile);
			}
		}
		
		public void Dispose()
		{
			pc?.Dispose();
			performanceLogger?.Dispose();
			taskScheduler?.Dispose();
		}

		private void HandleException(UnhandledExceptionEventArgs args, Action<string> notifyCallback)
		{
			var e = (Exception)args.ExceptionObject;
			HandleException(e, notifyCallback);
		}

		private void HandleException(Exception e, Action<string> notifyCallback, int level = 0)
		{
			var trace = new StackTrace(e, true);
			var bottomFrame = trace.GetFrame(0);

			var indents = string.Concat(Enumerable.Repeat("  ", level));

			var aggr = e as AggregateException;
			if (aggr != null)
			{
				var message = $"{indents}An unhandled AggregateException occurred in file: {bottomFrame.GetFileName()}:{bottomFrame.GetFileLineNumber()} - Sub-exceptions: ";
				notifyCallback(message);
				Logger.Log(this, message, LogLevel.Error);
				foreach (var inner in aggr.InnerExceptions)
				{
					HandleException(inner, notifyCallback, ++level);
				}
			}
			else
			{
				var message = $"{indents}An unhandled exception occured: {e.GetType().Name} - {e.Message} - in file: {bottomFrame.GetFileName()}:{bottomFrame.GetFileLineNumber()}";
				notifyCallback(message);
				Logger.Log(this, message, LogLevel.Error);
				if (e.InnerException != null)
				{
					HandleException(e.InnerException, notifyCallback, ++level);
				}
				else
				{
					foreach (var frame in trace.GetFrames())
					{
						Logger.Log(this, $"{indents}  -> in {frame}", LogLevel.Error);
					}
				}
			}
		}

		internal void StartPerformanceLogging()
		{
			if (!ConfigManager.Config.LogPerformance)
			{
				return;
			}
			var selfProc = Process.GetCurrentProcess();
			pc = new PerformanceCounter
			{
				CategoryName = "Process",
				CounterName = "Working Set - Private",
				InstanceName = selfProc.ProcessName
			};

			Logger.Log(this, "Logging performance statistics to " + PerfLogFile, LogLevel.Info);

			taskScheduler = new Timer { Interval = 2000 };
			taskScheduler.Start();
			taskScheduler.Elapsed += (source, eventArgs) =>
			{
				var mem = (long)(pc.NextValue() / 1024);
				// TODO: Find a better way to get access to these values
				//var users = ircInterface.TotalUserCount;
				//var chans = ircInterface.ChannelCount;
				performanceLogger.Log(mem, 0, 0);
			};
		}
	}
}
