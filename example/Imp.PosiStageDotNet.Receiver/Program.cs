using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
