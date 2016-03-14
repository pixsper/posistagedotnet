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
using System.Collections.Generic;
using System.Diagnostics;
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
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	public class PsnServer : IDisposable
	{
		public const int VersionHigh = 2;
		public const int VersionLow = 1;

		public const int MaxPacketLength = 1500;

		public const string DefaultMulticastIp = "236.10.10.10";
		public const int DefaultPort = 56565;

		public const double DefaultDataSendFrequency = 60d;
		public const double DefaultInfoSendFrequency = 1d;

		private readonly UdpSocketClient _udpClient = new UdpSocketClient();

		private IEnumerable<PsnTracker> _trackers; 

		private Timer _dataTimer;
		private Timer _infoTimer;
		private Stopwatch _timeStampReference = new Stopwatch();

		private byte _frameId;

		private bool _isDisposed;

		/// <summary>
		///     Constructs with default multicast IP, port number and data/info packet send frequencies
		/// </summary>
		/// <param name="systemName">Name used to identify this server</param>
		public PsnServer([NotNull] string systemName)
			: this(systemName, DefaultMulticastIp, DefaultPort, DefaultDataSendFrequency, DefaultInfoSendFrequency) { }

		/// <summary>
		///     Constructs with default multicast IP and port number
		/// </summary>
		/// <param name="systemName">Name used to identify this server</param>
		/// <param name="dataSendFrequency">Custom frequency in Hz at which data packets are sent</param>
		/// <param name="infoSendFrequency">Custom frequency in Hz at which info packets are sent</param>
		public PsnServer([NotNull] string systemName, double dataSendFrequency, double infoSendFrequency)
			: this(systemName, DefaultMulticastIp, DefaultPort, dataSendFrequency, infoSendFrequency) { }

		/// <summary>
		///     Constructs with default data/info packet send frequencies
		/// </summary>
		/// <param name="systemName">Name used to identify this server</param>
		/// <param name="customMulticastIp">Custom multicast IP to send data to</param>
		/// <param name="customPort">Custom UDP port number to send data to</param>
		public PsnServer([NotNull] string systemName, [NotNull] string customMulticastIp, int customPort)
			: this(systemName, customMulticastIp, customPort, DefaultDataSendFrequency, DefaultInfoSendFrequency) { }

		/// <summary>
		///     Constructs with custom values for all parameters
		/// </summary>
		/// <param name="systemName">Name used to identify this server</param>
		/// <param name="dataSendFrequency">Custom frequency in Hz at which data packets are sent</param>
		/// <param name="infoSendFrequency">Custom frequency in Hz at which info packets are sent</param>
		/// <param name="customMulticastIp">Custom multicast IP to send data to</param>
		/// <param name="customPort">Custom UDP port number to send data to</param>
		public PsnServer([NotNull] string systemName, double dataSendFrequency, double infoSendFrequency,
			[NotNull] string customMulticastIp, int customPort)
			: this(systemName, customMulticastIp, customPort, dataSendFrequency, infoSendFrequency) { }


		private PsnServer([NotNull] string systemName, [NotNull] string multicastIp, int port, double dataSendFrequency, double infoSendFrequency)
		{
			if (systemName == null)
				throw new ArgumentNullException(nameof(systemName));

			SystemName = systemName;

			if (string.IsNullOrWhiteSpace(multicastIp))
				throw new ArgumentException("customMulticastIp cannot be null or empty");

			MulticastIp = multicastIp;

			if (port < ushort.MinValue + 1 || port > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(port), port,
					$"customPort must be in range {ushort.MinValue + 1}-{ushort.MaxValue}");

			Port = port;

			if (dataSendFrequency <= 0)
				throw new ArgumentOutOfRangeException(nameof(dataSendFrequency), dataSendFrequency,
					"dataSendFrequency must be greater than 0");

			DataSendFrequency = dataSendFrequency;

			if (infoSendFrequency <= 0)
				throw new ArgumentOutOfRangeException(nameof(infoSendFrequency), infoSendFrequency,
					"infoSendFrequency must be greater than 0");

			InfoSendFrequency = infoSendFrequency;
		}


		/// <summary>
		///		System name used to identify this PosiStageNet server
		/// </summary>
		public string SystemName { get; }

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

		public TimeSpan CurrentTimeStamp => TimeSpan.FromMilliseconds(_timeStampReference.ElapsedMilliseconds);

		/// <summary>
		///     Rate in Hz at which data packets are sent
		/// </summary>
		public double DataSendFrequency { get; }

		/// <summary>
		///     Rate in Hz at which info packets are sent
		/// </summary>
		public double InfoSendFrequency { get; }

		/// <summary>
		///		Set the collection of trackers to send data and info packets for
		/// </summary>
		public void SetTrackers([CanBeNull] IEnumerable<PsnTracker> trackers) => _trackers = trackers;

		/// <summary>
		///		Send a custom PsnPacketChunk
		/// </summary>
		public void SendCustomPacket([NotNull] PsnPacketChunk chunk)
		{
			if (chunk == null)
				throw new ArgumentNullException(nameof(chunk));

			var bytes = chunk.ToByteArray();

			if (bytes.Length > MaxPacketLength)
				throw new ArgumentException($"Serialized chunk length ({bytes.Length}) is longer than maximum PosiStageNet packet length ({MaxPacketLength})");

			_udpClient.SendAsync(bytes);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (IsSending)
					StopSendingAsync().Wait();

				_udpClient.Dispose();
			}

			_isDisposed = true;
		}

		/// <summary>
		///     Joins the server to the multicast IP and starts sending PosiStageNet packets
		/// </summary>
		/// <returns></returns>
		public async Task StartSendingAsync()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(PsnServer));

			await _udpClient.ConnectAsync(MulticastIp, Port).ConfigureAwait(false);

			IsSending = true;
			_timeStampReference.Restart();
			_dataTimer = new Timer(sendData, 0, (int)(1000d / DataSendFrequency));
			_infoTimer = new Timer(sendInfo, 0, (int)(1000d / InfoSendFrequency));
		}

		/// <summary>
		///     Removes the server from the multicast group and stops sending PosiStageNet packets
		/// </summary>
		public async Task StopSendingAsync()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(PsnServer));

			if (!IsSending)
				throw new InvalidOperationException("Cannot stop sending, client is not currently sending");

			_dataTimer.Dispose();
			_dataTimer = null;
			_infoTimer.Dispose();
			_infoTimer = null;
			_timeStampReference.Reset();

			await _udpClient.DisconnectAsync().ConfigureAwait(false);

			IsSending = false;
		}

		protected virtual void OnSendInfo([NotNull] IEnumerable<PsnTracker> trackers)
		{
			var systemNameChunk = new PsnInfoSystemNameChunk(SystemName);
			var trackerChunks = trackers.Select(t => new PsnInfoTrackerChunk(t.TrackerId, t.ToInfoTrackerChunks()));

			var trackerListChunks = new List<PsnInfoTrackerListChunk>();
			var currentTrackerList = new List<PsnInfoTrackerChunk>();

			int trackerListLength = 0;
			int maxTrackerListLength = MaxPacketLength -
								  (PsnChunk.ChunkHeaderLength                       // Root Chunk-Header
								   + PsnInfoHeaderChunk.StaticChunkAndHeaderLength  // Packet Header Chunk
								   + systemNameChunk.ChunkAndHeaderLength           // System Name Chunk
								   + PsnChunk.ChunkHeaderLength);                   // Tracker List Chunk-Header

			foreach (var chunk in trackerChunks)
			{
				if (trackerListLength <= maxTrackerListLength)
				{
					currentTrackerList.Add(chunk);
					trackerListLength += chunk.ChunkAndHeaderLength;
				}
				else
				{
					trackerListChunks.Add(new PsnInfoTrackerListChunk(currentTrackerList));
					currentTrackerList = new List<PsnInfoTrackerChunk>();
					trackerListLength = 0;
				}
			}

			trackerListChunks.Add(new PsnInfoTrackerListChunk(currentTrackerList));

			ulong timestamp = (ulong)_timeStampReference.ElapsedMilliseconds;

			foreach (var trackerListChunk in trackerListChunks)
			{
				var packet = new PsnInfoPacketChunk(
					new PsnInfoHeaderChunk(timestamp, VersionHigh, VersionLow, _frameId, trackerListChunks.Count),
					systemNameChunk, trackerListChunk);

				var data = packet.ToByteArray();
				Debug.Assert(data.Length <= MaxPacketLength);

				_udpClient.SendAsync(data);
			}
		}

		protected virtual void OnSendData([NotNull] IEnumerable<PsnTracker> trackers)
		{
			var trackerChunks = trackers.Select(t => new PsnDataTrackerChunk(t.TrackerId, t.ToDataTrackerChunks()));

			var trackerListChunks = new List<PsnDataTrackerListChunk>();
			var currentTrackerList = new List<PsnDataTrackerChunk>();

			int trackerListLength = 0;
			int maxTrackerListLength = MaxPacketLength -
								  (PsnChunk.ChunkHeaderLength                       // Root Chunk-Header
								   + PsnDataHeaderChunk.StaticChunkAndHeaderLength  // Packet Header Chunk
								   + PsnChunk.ChunkHeaderLength);                   // Tracker List Chunk-Header

			foreach (var chunk in trackerChunks)
			{
				if (trackerListLength <= maxTrackerListLength)
				{
					currentTrackerList.Add(chunk);
					trackerListLength += chunk.ChunkAndHeaderLength;
				}
				else
				{
					trackerListChunks.Add(new PsnDataTrackerListChunk(currentTrackerList));
					currentTrackerList = new List<PsnDataTrackerChunk>();
					trackerListLength = 0;
				}
			}

			trackerListChunks.Add(new PsnDataTrackerListChunk(currentTrackerList));

			ulong timestamp = (ulong)_timeStampReference.ElapsedMilliseconds;

			foreach (var trackerListChunk in trackerListChunks)
			{
				var packet = new PsnDataPacketChunk(
					new PsnDataHeaderChunk(timestamp, VersionHigh, VersionLow, _frameId, trackerListChunks.Count),
					trackerListChunk);

				var data = packet.ToByteArray();
				Debug.Assert(data.Length <= MaxPacketLength);

				_udpClient.SendAsync(data);
			}

			++_frameId;
		}

		private void sendInfo()
		{
			// Copy local reference for threading reasons
			var trackers = _trackers;

			if (trackers == null)
				return;

			OnSendInfo(trackers);
		}

		private void sendData()
		{
			// Copy local reference for threading reasons
			var trackers = _trackers;

			if (trackers == null)
				return;
			
			OnSendData(trackers);
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