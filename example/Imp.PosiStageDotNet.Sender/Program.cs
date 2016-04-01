using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Imp.PosiStageDotNet.Server
{
	public class Program
	{
		static readonly Random Random = new Random();

		public static void Main(string[] args)
		{
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("Imp.PosiStageDotNet.Server Example");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine();

			PsnServer server;

			switch (args.Length)
			{
				case 0:
					server = new PsnServer(Environment.MachineName);
					Console.WriteLine($"Sending on default multicast IP '{PsnServer.DefaultMulticastIp}', default port {PsnServer.DefaultPort}");
					break;

				case 1:
				case 2:
					{
						IPAddress ip;

						if (!IPAddress.TryParse(args[0], out ip))
						{
							Console.WriteLine($"Invalid IP address value '{args[0]}'");
							return;
						}

						int port;

						if (args.Length == 2)
						{
							if (!int.TryParse(args[1], out port))
							{
								Console.WriteLine($"Invalid UDP port value '{args[1]}'");
								return;
							}

							if (port < ushort.MinValue + 1 || port > ushort.MaxValue)
							{
								Console.WriteLine($"UDP port value out of valid range '{args[1]}'");
								return;
							}
						}
						else
						{
							port = PsnServer.DefaultPort;
						}

						server = new PsnServer(Environment.MachineName, ip.ToString(), port);
						Console.WriteLine(
							$"Sending on custom multicast IP '{PsnServer.DefaultMulticastIp}', custom port {PsnServer.DefaultPort}");
					}
					break;

				default:
					Console.WriteLine("Invalid args. Format is 'Imp.PosiStageDotNet.Server [CustomMulticastIP] [CustomPort]");
					Console.WriteLine("E.g. 'Imp.PosiStageDotNet.Server 236.10.10.10 56565");
					return;
			}

			Console.WriteLine("Press any key to exit");
			Console.WriteLine("");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("");

			server.SetTrackers(createTrackers());
			server.StartSendingAsync().Wait();
	
			while (!Console.KeyAvailable)
			{
				server.SetTrackers(createTrackers());
				Thread.Sleep(1000 / 60);
			}

			Console.WriteLine("");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("");

			Console.WriteLine("Disposing server...");

			server.Dispose();

			Console.WriteLine("Server disposed. Exiting...");
			Console.WriteLine("");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("");
		}

		private static IEnumerable<PsnTracker> createTrackers()
		{
			return new[]
				{
					new PsnTracker(0, "Tracker 0", randomValue(), randomValue(), randomValue()),
					new PsnTracker(1, "Tracker 1", randomValue(), randomValue(), randomValue()),
					new PsnTracker(2, "Tracker 2", randomValue(), randomValue(), randomValue()),
					new PsnTracker(3, "Tracker 3", randomValue(), randomValue(), randomValue()),
					new PsnTracker(4, "Tracker 4", randomValue(), randomValue(), randomValue()),
					new PsnTracker(5, "Tracker 5", randomValue(), randomValue(), randomValue()),
				};
		}


		private static Tuple<float, float, float> randomValue()
		{
			return Tuple.Create((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble());
		}
	}
}
