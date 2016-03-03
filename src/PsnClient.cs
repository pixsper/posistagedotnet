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
using Imp.PosiStageDotNet.Chunks;
using JetBrains.Annotations;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace Imp.PosiStageDotNet
{
	/// <summary>
	/// Client for receiving PosiStageNet UDP packets
	/// </summary>
	[PublicAPI]
	public class PsnClient : IDisposable
	{
		public const string DefaultMulticastIp = "236.10.10.10";
		public const int DefaultPort = 56565;

		private readonly UdpSocketMulticastClient _udpClient = new UdpSocketMulticastClient();

		/// <summary>
		/// Constructs with the PosiStageNet default multicast IP and port number
		/// </summary>
		public PsnClient()
		{
			ListenMulticastIp = DefaultMulticastIp;
			ListenPort = DefaultPort;
		}

		/// <summary>
		/// Constructs with a custom multicast IP and port number
		/// </summary>
		/// <param name="customMulticastIp"></param>
		/// <param name="customPort"></param>
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

		
		/// <summary>
		/// Called when a PosiStageNet info packet is received
		/// </summary>
		public event EventHandler<PsnInfoPacketReceived> InfoPacketReceived;

		/// <summary>
		/// called when a PosiStageNet data packet is received
		/// </summary>
		public event EventHandler<PsnDataPacketReceived> DataPacketReceived;

		/// <summary>
		/// The multicast IP address the client is listening for packets on
		/// </summary>
		public string ListenMulticastIp { get; }

		/// <summary>
		/// The UDP port the client is listening for packets on
		/// </summary>
		public int ListenPort { get; }

		/// <summary>
		/// The IP address of the network adapter in use by the client, or null if no adapter is set
		/// </summary>
		[CanBeNull]
		public string AdapterIp { get; private set; }

		/// <summary>
		/// The current state of the client
		/// </summary>
		public bool IsListening { get; private set; }

		/// <summary>
		/// Joins the client to the multicast IP and starts listening for PosiStageNet packets
		/// </summary>
		/// <param name="adapterIp">The IP of the local network adapter to be used by the client, or null for all adapters</param>
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

		public async Task StopListeningAsync()
		{
			if (!IsListening)
				throw new InvalidOperationException("Cannot stop listening, client is not currently listening");

			_udpClient.MessageReceived -= messageReceived;
			await _udpClient.DisconnectAsync().ConfigureAwait(false);

			IsListening = false;
			AdapterIp = null;
		}

		public void Dispose()
		{
			_udpClient.Dispose();
		}

		private void messageReceived(object sender, UdpSocketMessageReceivedEventArgs args)
		{
			var chunk = PsnChunk.FromByteArray(args.ByteData);

			if (chunk == null)
				return;

			switch ((PsnPacketChunkId)chunk.ChunkId)
			{
				case PsnPacketChunkId.PsnInfoPacket:
					InfoPacketReceived?.Invoke(this, new PsnInfoPacketReceived((PsnInfoPacketChunk)chunk, args.RemoteAddress, args.RemotePort));
					break;
				case PsnPacketChunkId.PsnDataPacket:
					DataPacketReceived?.Invoke(this, new PsnDataPacketReceived((PsnDataPacketChunk)chunk, args.RemoteAddress, args.RemotePort));
					break;
			}
		}


		public class PsnInfoPacketReceived : EventArgs
		{
			internal PsnInfoPacketReceived(PsnInfoPacketChunk packet, string remoteAddress, string remotePort)
			{
				Packet = packet;
				RemoteAddress = remoteAddress;
				RemotePort = remotePort;
			}

			public PsnInfoPacketChunk Packet { get; }
			public string RemoteAddress { get; }
			public string RemotePort { get; }

			public override string ToString() => $"PsnInfoPacketReceived: RemoteAddress {RemoteAddress}, RemotePort {RemotePort}";
		}

		public class PsnDataPacketReceived : EventArgs
		{
			internal PsnDataPacketReceived(PsnDataPacketChunk packet, string remoteAddress, string remotePort)
			{
				Packet = packet;
				RemoteAddress = remoteAddress;
				RemotePort = remotePort;
			}

			public PsnDataPacketChunk Packet { get; }
			public string RemoteAddress { get; }
			public string RemotePort { get; }

			public override string ToString() => $"PsnDataPacketReceived: RemoteAddress {RemoteAddress}, RemotePort {RemotePort}";
		}
	}
}