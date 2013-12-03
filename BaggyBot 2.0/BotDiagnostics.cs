using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Timers;

namespace BaggyBot
{
	class BotDiagnostics : IDisposable
	{
		private IrcInterface ircInterface;
		private Timer taskScheduler;
		private Process selfProc;
		private PerformanceCounter pc = new PerformanceCounter();
		private const string perfLogFile = "performance_log.csv";
		private PerfLogger perfLogger = new PerfLogger(perfLogFile);

		private List<Exception> exceptions = new List<Exception>();
		public List<Exception> Exceptions
		{
			get { return exceptions; }
		}

		public void Dispose()
		{
			pc.Dispose();
			perfLogger.Dispose();
			taskScheduler.Dispose();
		}

		public BotDiagnostics(IrcInterface ircInterface)
		{
			//AppDomain.CurrentDomain.UnhandledException += HandleException;
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
			long mem = selfProc.PrivateMemorySize64;
			var trace = new StackTrace(e, true);
			var bottomFrame = trace.GetFrame(0);

			string message = "A fatal unhandled exception occured: " + e.GetType().Name + " - " + e.Message + " - stacktrace: " + bottomFrame.GetFileName() + " at line " + bottomFrame.GetFileLineNumber();

			ircInterface.SendMessage(Settings.Instance["operator_nick"], message);
			Logger.Log(message, LogLevel.Error);
			Logger.Log("Private memory size: " + mem + " bytes.", LogLevel.Info);
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
				perfLogger.Log(mem, chans, users);
			};
		}
	}
}
