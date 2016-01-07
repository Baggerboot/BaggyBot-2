using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace BaggyBot
{
	/// <summary>
	/// This class gathers data about the bot's performance and sends it to a logger so it can be written to a log file.
	/// </summary>
	class BotDiagnostics : IDisposable
	{
		private readonly IrcInterface ircInterface;
		private readonly Timer taskScheduler;
		private readonly PerformanceCounter pc = new PerformanceCounter();
		private const string PerfLogFile = "performance_log.csv";
		private readonly PerformanceLogger performanceLogger = new PerformanceLogger(PerfLogFile);
		public List<PerformanceObject> PerformanceLog
		{
			get
			{
				return performanceLogger.PerformanceLog;
			}
		}

		public void Dispose()
		{
			pc.Dispose();
			performanceLogger.Dispose();
			taskScheduler.Dispose();
		}

		public BotDiagnostics(IrcInterface ircInterface)
		{
			this.ircInterface = ircInterface;

			var selfProc = Process.GetCurrentProcess();
			pc.CategoryName = "Process";
			pc.CounterName = "Working Set - Private";
			pc.InstanceName = selfProc.ProcessName;

			AppDomain.CurrentDomain.UnhandledException += HandleException;
			

			taskScheduler = new Timer {Interval = 2000};
		}

		private void HandleException(Object sender, UnhandledExceptionEventArgs args)
		{
			var e = (Exception)args.ExceptionObject;
			var trace = new StackTrace(e, true);
			var bottomFrame = trace.GetFrame(0);

			var message = "A fatal unhandled exception occured: " + e.GetType().Name + " - " + e.Message + " - in file: " + bottomFrame.GetFileName() + ":" + bottomFrame.GetFileLineNumber();

			ircInterface.NotifyOperator(message);
			Logger.Log(this, message, LogLevel.Error);
		}

		internal void StartPerformanceLogging()
		{
			Logger.Log(this, "Logging performance statistics to " + PerfLogFile, LogLevel.Info);
			taskScheduler.Start();
			taskScheduler.Elapsed += (source, eventArgs) =>
			{
				var mem = (long)(pc.NextValue() / 1024);
				var users = ircInterface.TotalUserCount;
				var chans = ircInterface.ChannelCount;
				performanceLogger.Log(mem, chans, users);
			};
		}
	}
}
