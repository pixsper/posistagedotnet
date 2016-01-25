using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;
using Sockets.Plugin;

namespace Imp.PosiStageDotNet
{
	[PublicAPI]
	public sealed class PsnServer : IDisposable
	{
		public const string DefaultMulticastIp = "236.10.10.10";
		public const int DefaultPort = 56565;

		private bool _isDisposed;

		private readonly UdpSocketMulticastClient _socket = new UdpSocketMulticastClient();
		private readonly Timer _timer;

		
		public PsnServer(int port = DefaultPort, string multicastIp = DefaultMulticastIp)
		{
			if (port < ushort.MinValue || port > ushort.MaxValue)
				throw new ArgumentException("port must be in valid UDP port range 0-65535", nameof(port));

			Port = port;

			if (string.IsNullOrWhiteSpace(multicastIp))
				throw new ArgumentException("multicastIp must contain a valid multicast IP address", nameof(multicastIp));

			MulticastIp = multicastIp;

			_timer = new Timer(sendData, 0, 16);
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_timer.Dispose();
			_socket.Dispose();
			_isDisposed = true;
		}

		public Task StartAsync()
		{
			return _socket.JoinMulticastGroupAsync(MulticastIp, Port);
		}

		public string MulticastIp { get; }
		public int Port { get; }

		private void sendData()
		{
			
		}
	}
}
