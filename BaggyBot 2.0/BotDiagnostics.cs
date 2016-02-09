using System;
using System.Diagnostics;
using System.Timers;
using BaggyBot.DataProcessors.IO;

namespace BaggyBot
{
	/// <summary>
	/// This class gathers data about the bot's performance and sends it to a logger so it can be written to a log file.
	/// </summary>
	internal class BotDiagnostics : IDisposable
	{
		private const string PerfLogFile = "performance_log.csv";
		private readonly IrcInterface ircInterface;
		private Timer taskScheduler;
		private PerformanceCounter pc;
		private readonly PerformanceLogger performanceLogger = new PerformanceLogger(PerfLogFile);

		public BotDiagnostics(IrcInterface ircInterface)
		{
			this.ircInterface = ircInterface;

			AppDomain.CurrentDomain.UnhandledException += HandleException;
		}
		
		public void Dispose()
		{
			pc.Dispose();
			performanceLogger.Dispose();
			taskScheduler.Dispose();
		}

		private void HandleException(object sender, UnhandledExceptionEventArgs args)
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
			var selfProc = Process.GetCurrentProcess();
			pc = new PerformanceCounter();
			pc.CategoryName = "Process";
			pc.CounterName = "Working Set - Private";
			pc.InstanceName = selfProc.ProcessName;

			Logger.Log(this, "Logging performance statistics to " + PerfLogFile, LogLevel.Info);

			taskScheduler = new Timer { Interval = 2000 };
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
