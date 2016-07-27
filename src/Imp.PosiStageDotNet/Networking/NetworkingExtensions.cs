using System.Net;
using System.Net.Sockets;

namespace Imp.PosiStageDotNet.Networking
{
	internal static class NetworkingExtensions
	{
		public static bool IsIPv4Multicast(this IPAddress ipAddress)
		{
			if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
				return false;

			var ipBytes = ipAddress.GetAddressBytes();
			return ipBytes[0] >= 224 && ipBytes[1] <= 239;
		}
	}
}
