using System;
using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Imp.PosiStageDotNet.Networking
{
	/// <summary>
	///     Helper class wrapping <see cref="UdpClient"/> to provide event-based UDP service
	/// </summary>
	internal class UdpService : IDisposable
	{
		private bool _isDisposed;

		private readonly UdpClient _udpClient;
		private CancellationTokenSource _cancellationTokenSource;

		public UdpService(IPEndPoint localEndPoint)
		{
			if (localEndPoint is null)
				throw new ArgumentNullException(nameof(localEndPoint));

			LocalEndPoint = localEndPoint;

			_udpClient = new UdpClient
			{
				EnableBroadcast = true
			};

			_udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_udpClient.Client.Bind(LocalEndPoint);
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			if (IsListening)
				StopListening();

#if !NET45
			_udpClient.Dispose();
#endif

			_isDisposed = true;
		}

		public event EventHandler<UdpReceiveResult> MessageReceived;


		public bool IsListening { get; private set; }

		public IPEndPoint LocalEndPoint { get; }

		public ImmutableHashSet<IPAddress> MulticastGroups { get; private set; } = ImmutableHashSet<IPAddress>.Empty;

		public void StartListening()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			if (IsListening)
				throw new InvalidOperationException("Cannot start listening, UdpReceiver is already listening");

			_cancellationTokenSource = new CancellationTokenSource();
			Task.Run(() => receiveMessages(_cancellationTokenSource.Token));

			IsListening = true;
		}

		public void StopListening()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			if (!IsListening)
				throw new InvalidOperationException("Cannot stop listening, UdpReceiver is not currently listening");

			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;

			IsListening = false;
		}

		public void JoinMulticastGroup(IPAddress multicastIp)
		{
			if (multicastIp.AddressFamily != AddressFamily.InterNetwork)
				throw new ArgumentException("Not a valid IPv4 address", nameof(multicastIp));

			var ipBytes = multicastIp.GetAddressBytes();
			if (ipBytes[0] < 224 || ipBytes[1] > 239)
				throw new ArgumentException("Not a valid multicast address", nameof(multicastIp));

			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			if (MulticastGroups.Contains(multicastIp))
				throw new ArgumentException($"Already a member of multicast group {multicastIp}", nameof(multicastIp));

			_udpClient.JoinMulticastGroup(multicastIp);
			MulticastGroups = MulticastGroups.Add(multicastIp);
		}

		public void DropMulticastGroup(IPAddress multicastIp)
		{
			if (multicastIp.AddressFamily != AddressFamily.InterNetwork)
				throw new ArgumentException("Not a valid IPv4 address", nameof(multicastIp));

			var ipBytes = multicastIp.GetAddressBytes();
			if (ipBytes[0] < 224 || ipBytes[1] > 239)
				throw new ArgumentException("Not a valid multicast address", nameof(multicastIp));

			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			if (!MulticastGroups.Contains(multicastIp))
				throw new ArgumentException($"Not a member of multicast group {multicastIp}", nameof(multicastIp));

			_udpClient.DropMulticastGroup(multicastIp);
			MulticastGroups = MulticastGroups.Remove(multicastIp);
		}

		public Task<int> SendAsync(byte[] data, IPEndPoint endPoint)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			return _udpClient.SendAsync(data, data.Length, endPoint);
		}

		public Task<int> SendAsync(byte[] data, int length, IPEndPoint endPoint)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			return _udpClient.SendAsync(data, length, endPoint);
		}

		private async void receiveMessages(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				bool didReceive = false;
				UdpReceiveResult message;

				try
				{
					message = await _udpClient.ReceiveAsync().ConfigureAwait(false);
					didReceive = true;
				}
				catch
				{
					// Exception may be thrown by cancellation
					if (!cancellationToken.IsCancellationRequested)
						throw;
				}

				// If nothing received, must have been cancelled
				if (!didReceive)
					return;

				MessageReceived?.Invoke(this, message);
			}
		}
	}
}
