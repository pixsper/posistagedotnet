using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DBDesign.PosiStageDotNet.Networking
{
	/// <summary>
	///     Helper class wrapping <see cref="UdpClient"/> to provide event-based UDP service
	/// </summary>
	internal class UdpService : IDisposable
	{
		private bool _isDisposed;

		private readonly UdpClient _udpClient;
		
        private Task _receiveTask = Task.CompletedTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly HashSet<IPAddress> _multicastGroups = new HashSet<IPAddress>();

		public UdpService(IPEndPoint localEndPoint)
		{
            LocalEndPoint = localEndPoint ?? throw new ArgumentNullException(nameof(localEndPoint));

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
				StopListeningAsync().Wait();

			_udpClient.Dispose();

            _isDisposed = true;
		}

		public event EventHandler<UdpReceiveResult> PacketReceived;


		public bool IsListening { get; private set; }

		public IPEndPoint LocalEndPoint { get; }

        public IReadOnlyCollection<IPAddress> MulticastGroups => _multicastGroups;



		public void StartListening()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			if (IsListening)
				throw new InvalidOperationException($"Cannot start listening, {nameof(UdpService)} is already listening");

            _receiveTask = listenAsync(_cancellationTokenSource.Token);

			IsListening = true;
		}

		public async Task StopListeningAsync()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);

			if (!IsListening)
				throw new InvalidOperationException($"Cannot stop listening, {nameof(UdpService)} is not currently listening");

            _cancellationTokenSource.Cancel();
            
            await _receiveTask.ConfigureAwait(false);
            _receiveTask = Task.CompletedTask;

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

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

			if (_multicastGroups.Contains(multicastIp))
				throw new ArgumentException($"Already a member of multicast group {multicastIp}", nameof(multicastIp));

			if (LocalEndPoint.Address.Equals(IPAddress.Any))
                _udpClient.JoinMulticastGroup(multicastIp);
			else
			    _udpClient.JoinMulticastGroup(multicastIp, LocalEndPoint.Address);

            _multicastGroups.Add(multicastIp);
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

			if (!_multicastGroups.Contains(multicastIp))
				throw new ArgumentException($"Not a member of multicast group {multicastIp}", nameof(multicastIp));

			_udpClient.DropMulticastGroup(multicastIp);
			_multicastGroups.Remove(multicastIp);
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

        private async Task listenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var task = _udpClient.ReceiveAsync();

                    var tcs = new TaskCompletionSource<bool>();
                    using (cancellationToken.Register(s => ((TaskCompletionSource<bool>) s).TrySetResult(true), tcs))
                    {
                        if (task != await Task.WhenAny(task, tcs.Task))
                            return;
                    }

                    PacketReceived?.Invoke(this, task.Result);
                }
                catch (SocketException ex)
                {

                }
            }
        }
	}
}
