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
using System.Threading;
using System.Threading.Tasks;
using Imp.PosiStageDotNet.Chunks;
using JetBrains.Annotations;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace Imp.PosiStageDotNet
{
	/// <summary>
	///     Class to serialize and repeatedly send PosiStageNet Data and Info packets at defined rates
	/// </summary>
	[PublicAPI]
	public sealed class PsnServer : IDisposable
	{
		public const string DefaultMulticastIp = "236.10.10.10";
		public const int DefaultPort = 56565;

		public const int DefaultDataSendRateHz = 60;
		public const int DefaultInfoSendRateHz = 1;

		private readonly UdpSocketMulticastClient _udpClient = new UdpSocketMulticastClient();

		private PsnDataPacketChunk _dataPacket;
		private byte[] _dataPacketCachedBytes;
		private Timer _dataTimer;

		private PsnInfoPacketChunk _infoPacket;
		private byte[] _infoPacketCachedBytes;
		private Timer _infoTimer;

		private bool _isDisposed;

		/// <summary>
		///     Constructs with the PosiStageNet default multicast IP and port number
		/// </summary>
		/// <param name="adapterIp">IP address of local network adapter to use, or null to use all network adapters</param>
		/// <param name="dataSendRateHz">Rate in Hz at which to send data packets</param>
		/// <param name="infoSendRateHz">rate in Hz at which to send info packets</param>
		public PsnServer(string adapterIp = null, int dataSendRateHz = DefaultDataSendRateHz,
			int infoSendRateHz = DefaultInfoSendRateHz)
		{
			AdapterIp = adapterIp;
			DataSendRateHz = dataSendRateHz;
			InfoSendRateHz = infoSendRateHz;
			MulticastIp = DefaultMulticastIp;
			Port = DefaultPort;
		}

		/// <summary>
		///     Constructs with a custom multicast IP and port number
		/// </summary>
		/// <param name="customMulticastIp"></param>
		/// <param name="customPort"></param>
		/// <param name="adapterIp">IP address of local network adapter to use, or null to use all network adapters</param>
		/// <param name="dataSendRateHz">Rate in Hz at which to send data packets</param>
		/// <param name="infoSendRateHz">rate in Hz at which to send info packets</param>
		public PsnServer(string customMulticastIp, int customPort, string adapterIp = null,
			int dataSendRateHz = DefaultDataSendRateHz, int infoSendRateHz = DefaultInfoSendRateHz)
		{
			if (string.IsNullOrWhiteSpace(customMulticastIp))
				throw new ArgumentException("customMulticastIp cannot be null or empty");

			MulticastIp = customMulticastIp;

			if (customPort < ushort.MinValue + 1 || customPort > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(customPort), customPort,
					$"customPort must be in range {ushort.MinValue + 1}-{ushort.MaxValue}");

			Port = customPort;

			AdapterIp = adapterIp;
			DataSendRateHz = dataSendRateHz;
			InfoSendRateHz = infoSendRateHz;
		}

		/// <summary>
		///     The multicast IP address the server is sending packets to
		/// </summary>
		public string MulticastIp { get; }

		/// <summary>
		///     The UDP port the server is sending packets to
		/// </summary>
		public int Port { get; }

		/// <summary>
		///     The IP address of the network adapter in use by the server, or null if no adapter is set
		/// </summary>
		[CanBeNull]
		public string AdapterIp { get; }

		/// <summary>
		///     The current state of the server
		/// </summary>
		public bool IsSending { get; private set; }

		/// <summary>
		///     Refresh rate at which data packets are sent
		/// </summary>
		public int DataSendRateHz { get; }

		/// <summary>
		///     Refresh rate at which info packets are sent
		/// </summary>
		public int InfoSendRateHz { get; }

		/// <summary>
		///     The data packet to send at the data refresh rate
		/// </summary>
		public PsnDataPacketChunk DataPacket
		{
			get { return _dataPacket; }
			set
			{
				if (!ReferenceEquals(value, _dataPacket))
				{
					_dataPacket = value;
					_dataPacketCachedBytes = _dataPacket?.ToByteArray();
				}
			}
		}

		/// <summary>
		///     The info packet to send at the info refresh rate
		/// </summary>
		public PsnInfoPacketChunk InfoPacket
		{
			get { return _infoPacket; }
			set
			{
				if (!ReferenceEquals(value, _infoPacket))
				{
					_infoPacket = value;
					_infoPacketCachedBytes = _infoPacket?.ToByteArray();
				}
			}
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			if (IsSending)
				StopSendingAsync().Wait();

			_udpClient.Dispose();
			_isDisposed = true;
		}

		/// <summary>
		///     Joins the server to the multicast IP and starts sending PosiStageNet packets
		/// </summary>
		/// <returns></returns>
		public async Task StartSendingAsync()
		{
			ICommsInterface adapter = null;

			if (AdapterIp != null)
			{
				var interfaces = await CommsInterface.GetAllInterfacesAsync().ConfigureAwait(false);

				adapter = interfaces.FirstOrDefault(i => i.IpAddress == AdapterIp);

				if (adapter == null)
					throw new ArgumentException($"Adapter with IP of '{AdapterIp}' cannot be found");
			}

			await _udpClient.JoinMulticastGroupAsync(MulticastIp, Port, adapter).ConfigureAwait(false);

			IsSending = true;
			_dataTimer = new Timer(sendData, 0, 1000 / DefaultDataSendRateHz);
			_infoTimer = new Timer(sendInfo, 0, 1000 / DefaultInfoSendRateHz);
		}

		/// <summary>
		///     Removes the server from the multicast group and stops sending PosiStageNet packets
		/// </summary>
		public async Task StopSendingAsync()
		{
			if (!IsSending)
				throw new InvalidOperationException("Cannot stop sending, client is not currently sending");

			_dataTimer.Dispose();
			_dataTimer = null;
			_infoTimer.Dispose();
			_infoTimer = null;

			await _udpClient.DisconnectAsync().ConfigureAwait(false);

			IsSending = false;
		}

		private void sendData()
		{
			// Copy local reference for threading reasons
			var dataBytes = _dataPacketCachedBytes;

			if (dataBytes != null)
				_udpClient.SendMulticastAsync(dataBytes);
		}

		private void sendInfo()
		{
			// Copy local reference for threading reasons
			var infoBytes = _infoPacketCachedBytes;

			if (infoBytes != null)
				_udpClient.SendMulticastAsync(infoBytes);
		}



		internal class Timer : CancellationTokenSource, IDisposable
		{
			public Timer(Action callback, int dueTime, int period)
			{
				Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
				{
					var action = (Action)s;

					while (true)
					{
						if (IsCancellationRequested)
							break;

#pragma warning disable 4014
						Task.Run(() => action());
#pragma warning restore 4014

						await Task.Delay(period).ConfigureAwait(true);
					}
				}, callback, CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
					TaskScheduler.Default);
			}

			public new void Dispose()
			{
				Cancel();
			}
		}
	}
}