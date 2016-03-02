// This file is part of PosiStageDotNet.
// 
// PosiStageDotNet is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PosiStageDotNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with PosiStageDotNet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace Imp.PosiStageDotNet
{
	[PublicAPI]
	public class PsnClient : IDisposable
	{
		public const string DefaultMulticastIp = "236.10.10.10";
		public const int DefaultPort = 56565;

		private readonly UdpSocketMulticastClient _udpClient = new UdpSocketMulticastClient();

		public PsnClient(string customMulticastIp, int customPort)
		{
			if (string.IsNullOrWhiteSpace(customMulticastIp))
				throw new ArgumentException("customMulticastIp cannot be null or empty");

			ListenMulticastIp = customMulticastIp;

			if (customPort < ushort.MinValue || customPort > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(customPort), customPort,
					$"customPort must be in range {ushort.MinValue}-{ushort.MaxValue}");

			ListenPort = customPort;
		}

		public PsnClient()
		{
			ListenMulticastIp = DefaultMulticastIp;
			ListenPort = DefaultPort;
		}

		public event EventHandler<PsnChunk> PacketReceived;

		public string ListenMulticastIp { get; }
		public int ListenPort { get; }

		public string AdapterIp { get; private set; }
		public bool IsListening { get; private set; }

		public async Task StartListeningAsync(string adapterIp = null)
		{
			if (adapterIp != null)
			{
				var interfaces = await CommsInterface.GetAllInterfacesAsync().ConfigureAwait(false);

				var adapter = interfaces.FirstOrDefault(i => i.IpAddress == adapterIp);

				if (adapter == null)
					throw new ArgumentException($"Adapter with IP of '{adapterIp}' cannot be found");

				await _udpClient.JoinMulticastGroupAsync(ListenMulticastIp, ListenPort, adapter).ConfigureAwait(false);

				AdapterIp = adapterIp;
			}
			else
			{
				await _udpClient.JoinMulticastGroupAsync(ListenMulticastIp, ListenPort).ConfigureAwait(false);
			}

			IsListening = true;
			_udpClient.MessageReceived += messageReceived;
		}

		public void Dispose()
		{
			_udpClient.Dispose();
		}

		private void messageReceived(object sender, UdpSocketMessageReceivedEventArgs args)
		{
			PacketReceived?.Invoke(this, PsnChunk.FromByteArray(args.ByteData));
		}
	}
}