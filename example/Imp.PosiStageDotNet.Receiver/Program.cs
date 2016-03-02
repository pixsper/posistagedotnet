using System;
using System.Diagnostics;
using Imp.PosiStageDotNet.Chunks;

namespace Imp.PosiStageDotNet.Receiver
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new PsnClient();
			client.PacketReceived += packetReceived;

			client.StartListeningAsync().Wait();

			Console.ReadKey();
		}

		private static void packetReceived(object sender, PsnChunk e)
		{
			Console.WriteLine(e);
		}
	}
}
