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
				throw new ArgumentOutOfRangeException(nameof(customPort), customPort, $"customPort must be in range {ushort.MinValue}-{ushort.MaxValue}");

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
