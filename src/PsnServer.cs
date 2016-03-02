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
using System.Threading.Tasks;
using Sockets.Plugin;

namespace Imp.PosiStageDotNet
{
	[PublicAPI]
	public sealed class PsnServer : IDisposable
	{
		public const string DefaultMulticastIp = "236.10.10.10";
		public const int DefaultPort = 56565;

		private readonly UdpSocketMulticastClient _socket = new UdpSocketMulticastClient();
		private readonly Timer _timer;

		private bool _isDisposed;


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

		public string MulticastIp { get; }
		public int Port { get; }

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

		private void sendData() { }
	}
}