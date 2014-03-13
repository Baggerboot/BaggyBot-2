using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Timers;

namespace BaggyBot
{
	/// <summary>
	/// This class gathers data about the bot's performance and sends it to a logger so it can be written to a log file.
	/// </summary>
	class BotDiagnostics : IDisposable
	{
		private IrcInterface ircInterface;
		private Timer taskScheduler;
		private Process selfProc;
		private PerformanceCounter pc = new PerformanceCounter();
		private const string perfLogFile = "performance_log.csv";
		private PerformanceLogger performanceLogger = new PerformanceLogger(perfLogFile);
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

			selfProc = Process.GetCurrentProcess();
			pc.CategoryName = "Process";
			pc.CounterName = "Working Set - Private";
			pc.InstanceName = selfProc.ProcessName;

			taskScheduler = new Timer();
			taskScheduler.Interval = 2000;
		}

		private void HandleException(Object sender, UnhandledExceptionEventArgs args)
		{
			Exception e = (Exception)args.ExceptionObject;
			var trace = new StackTrace(e, true);
			var bottomFrame = trace.GetFrame(0);

			string message = "A fatal unhandled exception occured: " + e.GetType().Name + " - " + e.Message + " - in file: " + bottomFrame.GetFileName() + ":" + bottomFrame.GetFileLineNumber();

			ircInterface.NotifyOperator(message);
			Logger.Log(message, LogLevel.Error);
		}

		internal void StartPerformanceLogging()
		{
			Logger.Log("Logging performance statistics to " + perfLogFile, LogLevel.Info);
			taskScheduler.Start();
			taskScheduler.Elapsed += (source, eventArgs) =>
			{
				long mem = (long)(pc.NextValue() / 1024);
				int users = ircInterface.TotalUserCount;
				int chans = ircInterface.ChannelCount;
				performanceLogger.Log(mem, chans, users);
			};
		}
	}
}
