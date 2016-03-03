using System;
using System.Net;

namespace Imp.PosiStageDotNet.Receiver
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("Imp.PosiStageDotNet.Receiver Example");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine();

			PsnClient client;

			switch (args.Length)
			{
				case 0:
					client = new PsnClient();
					Console.WriteLine($"Listening on default multicast IP '{PsnClient.DefaultMulticastIp}', default port {PsnClient.DefaultPort}");
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
						port = PsnClient.DefaultPort;
					}

					client = new PsnClient(ip.ToString(), port);
					Console.WriteLine($"Listening on custom multicast IP '{PsnClient.DefaultMulticastIp}', custom port {PsnClient.DefaultPort}");
				}
					break;

				default:
					Console.WriteLine("Invalid args. Format is 'Imp.PosiStageDotNet.Receiver [CustomMulticastIP] [CustomPort]");
					Console.WriteLine("E.g. 'Imp.PosiStageDotNet.Receiver 236.10.10.10 56565");
					return;
			}

			Console.WriteLine("Press any key to exit");
			Console.WriteLine("");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("");

			client.InfoPacketReceived += infoPacketReceived;
			client.DataPacketReceived += dataPacketReceived;
			client.StartListeningAsync().Wait();

			Console.ReadKey();

			Console.WriteLine("");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("");

			Console.WriteLine("Disposing client...");

			client.Dispose();

			Console.WriteLine("Client disposed. Exiting...");
			Console.WriteLine("");
			Console.WriteLine(new string('*', Console.WindowWidth - 1));
			Console.WriteLine("");
		}

		private static void infoPacketReceived(object sender, PsnClient.PsnInfoPacketReceived e)
		{
			Console.WriteLine(e.Packet);
		}

		private static void dataPacketReceived(object sender, PsnClient.PsnDataPacketReceived e)
		{
			Console.WriteLine(e.Packet);
		}

	}
}
