using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace RemoteView
{
	class Program
	{
		static Mutex mutex = new Mutex(true, "-RemoteView-Mutex-");

		private static string ApplicationName
		{
			get
			{
				return Path.GetFileNameWithoutExtension(Application.ExecutablePath);
			}
		}

		/// <summary>
		/// Main method. Needs STAThreadAttribute as this App references System.Windows.Forms
		/// </summary>
		/// <param name="args"></param>
		[STAThreadAttribute]
		static int Main(String[] args)
		{

			// get configuration from command line parameters

			Configuration conf;
			try
			{
				conf = Configuration.create(args);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				return 1;
			}

			if (conf.Banner)
			{
				ShowBanner();
			}

			if (conf.Help)
			{
				ShowHelpMessage();
				return 0;
			}

			// make sure only one instance is online

			if (!conf.AllowMultiple && !InstanceIsUnique())
			{
				Console.Error.WriteLine("Only one instance of process allowed. User -m for muliple instances.");
				return 1;
			}

			// check if http listener is supported

			if (!HttpListener.IsSupported)
			{
				Console.Error.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
				return 1;
			}

			try {
				// run server
				RunServer(conf);
			} catch(Exception e) {
				Console.Error.WriteLine(e);
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Find out if there are one or more instances of this program running
		/// </summary>
		/// <returns>n processes</returns>
		private static bool InstanceIsUnique()
		{
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				mutex.ReleaseMutex();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Run server
		/// </summary>
		/// <param name="conf">server configuration</param>
		private static void RunServer(Configuration conf)
		{
			Console.Error.WriteLine("Running in {0} mode", conf.AllowWrite ? "remote control" : "view only");
			using (Server server = new Server(conf.AllowWrite).Start(conf.IpAddress, conf.Port))
			{
				if (server == null || !server.IsRunning())
				{
					Console.Error.WriteLine("Could not start server... Exiting.");
					return;
				}
				server.WaitForDisconnected();
				//server.Stop();
			}
		}

		private static void ShowBanner()
		{
			Console.WriteLine(Application.ProductName + " - Desktop sharing server");
			Console.WriteLine("Copyright (c) Joao Vilaca, 2013, Email: jvilaca@gmail.com");
			Console.WriteLine();
		}

		private static void ShowHelpMessage()
		{
			Console.WriteLine("Syntax: " + ApplicationName + " [Port to listen] [Options]");
			Console.WriteLine("Example: " + ApplicationName + " 6060 -b");
			Console.WriteLine("Options: -ip :\tBind ip;");
			Console.WriteLine("         -b  :\tDon't show banner message;");
			Console.WriteLine("         -m  :\tAllow multiple instances;");
			Console.WriteLine("         -r  :\tRead only;");
			Console.WriteLine("         -h  :\tHelp (This screen);");
			//Console.WriteLine("\t-i :\tInstall as Windows service");
			//Console.WriteLine("\t-u :\tUninstall as Windows service");
		}
	}
}
