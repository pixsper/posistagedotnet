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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DBDesign.PosiStageDotNet.Chunks;
using DBDesign.PosiStageDotNet.Networking;
using JetBrains.Annotations;

namespace DBDesign.PosiStageDotNet
{
	/// <summary>
	///     Client for receiving PosiStageNet UDP packets
	/// </summary>
	/// <example>
	///     // Setup
	///     var client = new PsnClient();
	///     client.TrackersUpdated += client_TrackersUpdated;
	///     await client.StartListeningAsync();
	/// 
	///     // Reading data
	///     foreach(var tracker in client.Trackers.Values)
	///     {
	///         Debug.WriteLine(pair.Value)
	///     }
	/// 
	///     // When finished
	///     client.Dispose();
	/// </example>
	[PublicAPI]
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	public class PsnClient : IDisposable
	{
		/// <summary>
		///     Standard IP address used by PosiStageNet
		/// </summary>
		public static readonly IPAddress DefaultMulticastIp = IPAddress.Parse("236.10.10.10");

		/// <summary>
		///     Standard port used by PosiStageNet
		/// </summary>
		public const int DefaultPort = 56565;

		private readonly List<PsnDataPacketChunk> _currentDataPacketChunks = new List<PsnDataPacketChunk>();
		private readonly List<PsnInfoPacketChunk> _currentInfoPacketChunks = new List<PsnInfoPacketChunk>();

		private readonly ConcurrentDictionary<int, PsnTracker> _trackers = new ConcurrentDictionary<int, PsnTracker>();

	    private readonly UdpService _udpService;

		private bool _isDisposed;

		private PsnDataHeaderChunk _lastDataPacketHeader;
		private PsnInfoHeaderChunk _lastInfoPacketHeader;



		/// <summary>
		///     Constructs with the PosiStageNet default multicast IP and port number
		/// </summary>
		/// <param name="localIp">IP of local network adapter to listen for multicast packets using</param>
		/// <param name="isStrict">If true, packets which are imperfect in any way will not be processed</param>
		public PsnClient([NotNull] IPAddress localIp, bool isStrict = true)
		{
			Trackers = new ReadOnlyDictionary<int, PsnTracker>(_trackers);

			MulticastIp = DefaultMulticastIp;
			Port = DefaultPort;
            LocalIp = localIp;
			IsStrict = isStrict;

			_udpService = new UdpService(new IPEndPoint(LocalIp, Port));
		}

	    /// <summary>
	    ///     Constructs with a custom multicast IP and port number
	    /// </summary>
	    /// /// <param name="localIp">IP of local network adapter to listen for multicast packets using</param>
	    /// <param name="customMulticastIp"></param>
	    /// <param name="customPort"></param>
        /// <param name="isStrict">If true, packets which are imperfect in any way will not be processed</param>
	    /// <exception cref="ArgumentException"></exception>
	    /// <exception cref="ArgumentOutOfRangeException"></exception>
	    public PsnClient([NotNull] IPAddress localIp, [NotNull] IPAddress customMulticastIp, int customPort, bool isStrict = true)
		{
	        Trackers = new ReadOnlyDictionary<int, PsnTracker>(_trackers);

			if (customMulticastIp == null)
				throw new ArgumentNullException(nameof(customMulticastIp));
			if (!customMulticastIp.IsIPv4Multicast())
				throw new ArgumentException("Not a valid IPv4 multicast address", nameof(customMulticastIp));
			MulticastIp = customMulticastIp;

			if (customPort < ushort.MinValue + 1 || customPort > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(customPort), customPort,
					$"customPort must be in range {ushort.MinValue + 1}-{ushort.MaxValue}");

			Port = customPort;

            LocalIp = localIp;
            IsStrict = isStrict;

			_udpService = new UdpService(new IPEndPoint(LocalIp, Port));
		}

		/// <summary>
		///     The multicast IP address the client is listening for packets on
		/// </summary>
		public IPAddress MulticastIp { get; }

		/// <summary>
		///     The UDP port the client is listening for packets on
		/// </summary>
		public int Port { get; }

		/// <summary>
		///     The IP address of the network adapter in use by the client, or null if no adapter is set
		/// </summary>
		[CanBeNull]
		public IPAddress LocalIp { get; }

		/// <summary>
		///     When true, packets which are missing any expected chunks or duplicate indexes will not be processed at all.
		///     If false, a best effort attempt will be made to process all packets.
		/// </summary>
		public bool IsStrict { get; }

		/// <summary>
		///     The current state of the client
		/// </summary>
		public bool IsListening { get; private set; }

		/// <summary>
		///     Dictionary of trackers keyed by tracker index
		/// </summary>
		public ReadOnlyDictionary<int, PsnTracker> Trackers { get; }

		/// <summary>
		///     System name of the remote PosiStageNet server, or null if no info packets have been received
		/// </summary>
		[CanBeNull]
		public string RemoteSystemName { get; private set; }

		/// <summary>
		///     Stops listening for data and releases network resources
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}



		/// <summary>
		///     Called when the value of <see cref="RemoteSystemName" /> is updated
		/// </summary>
		public event EventHandler<string> RemoteSystemNameUpdated;

		/// <summary>
		///     Called when a PosiStageNet data or info packet is received
		/// </summary>
		public event EventHandler<ReadOnlyDictionary<int, PsnTracker>> TrackersUpdated;

		/// <summary>
		///     Called when a PosiStageNet info packet is received
		/// </summary>
		public event EventHandler<PsnInfoPacketChunk> InfoPacketReceived;

		/// <summary>
		///     Called when a PosiStageNet data packet is received
		/// </summary>
		public event EventHandler<PsnDataPacketChunk> DataPacketReceived;

		/// <summary>
		///     Called when an incorrectly formatted PosiStageNet packet is received
		/// </summary>
		/// <remarks>Will only be called for packets which are correct enough to be deserialized, but are missing vital information</remarks>
		public event EventHandler<InvalidPacketsReceivedEventArgs> InvalidPacketReceived;

		/// <summary>
		///     Called when an unknown PosiStageNet packet is received
		/// </summary>
		public event EventHandler<PsnUnknownPacketChunk> UnknownPacketReceived;

		/// <summary>
		///     Joins the client to the multicast IP and starts listening for PosiStageNet packets
		/// </summary>
		/// <exception cref="ObjectDisposedException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void StartListening()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(PsnClient));

			_udpService.JoinMulticastGroup(MulticastIp);
			_udpService.PacketReceived += packetReceived;
			_udpService.StartListening();

			IsListening = true;
		   
		}

		/// <summary>
		///     Remove the client from the multicast group and stops listening for PosiStageNet packets
		/// </summary>
		/// <exception cref="ObjectDisposedException"></exception>
		/// <exception cref="InvalidOperationException">Cannot stop listening, client is not currently listening</exception>
		public void StopListening()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(PsnClient));

			if (!IsListening)
				throw new InvalidOperationException("Cannot stop listening, client is not currently listening");

			_udpService.PacketReceived -= packetReceived;
			_udpService.StopListeningAsync().Wait();
			_udpService.DropMulticastGroup(MulticastIp);

			IsListening = false;
		}

		/// <summary>
		///     Stops listening for data and disposes network resources
		/// </summary>
		/// <param name="isDisposing"></param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
	            if (IsListening)
		            StopListening();

				_udpService.Dispose();
			}

			_isDisposed = true;
		}



		/// <summary>
		///     Override to provide new behavior for receipt of all PosiStageNet packets
		/// </summary>
		protected virtual void OnPacketReceived(PsnPacketChunk packet)
		{
			switch (packet.ChunkId)
			{
				case PsnPacketChunkId.PsnInfoPacket:
				{
					var infoPacket = (PsnInfoPacketChunk)packet;
					OnInfoPacketReceived(infoPacket);
					InfoPacketReceived?.Invoke(this, infoPacket);
				}
					break;
				case PsnPacketChunkId.PsnDataPacket:
				{
					var dataPacket = (PsnDataPacketChunk)packet;
					OnDataPacketReceived(dataPacket);
					DataPacketReceived?.Invoke(this, dataPacket);
				}
					break;
				case PsnPacketChunkId.UnknownPacket:
				{
					var unknownPacket = (PsnUnknownPacketChunk)packet;
					OnUnknownPacketReceived(unknownPacket);
					UnknownPacketReceived?.Invoke(this, unknownPacket);
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		///     Override to provide new behavior for receipt of a PosiStageNet info packet
		/// </summary>
		protected virtual void OnInfoPacketReceived(PsnInfoPacketChunk infoPacket)
		{
			var headerChunk = getSingleChunk<PsnInfoHeaderChunk, PsnInfoPacketChunk>(infoPacket,
				(ushort)PsnInfoPacketChunkId.PsnInfoHeader,
				"Info", "info header");

			if (headerChunk == null)
				return;

			var systemNameChunk = getSingleChunk<PsnInfoSystemNameChunk, PsnInfoPacketChunk>(infoPacket,
				(ushort)PsnInfoPacketChunkId.PsnInfoSystemName,
				"Info", "system name");

			if (systemNameChunk == null)
				return;

			if (_currentInfoPacketChunks.Any())
			{
				if (headerChunk.TimeStamp != _lastInfoPacketHeader.TimeStamp
					|| headerChunk.FramePacketCount != _lastInfoPacketHeader.FramePacketCount)
				{
					InvalidPacketReceived?.Invoke(this,
						new InvalidPacketsReceivedEventArgs(_currentInfoPacketChunks, false,
							"Incomplete packet chunk discarded, did not receive all packets for info frame"));

					_currentInfoPacketChunks.Clear();
				}
			}

			_lastInfoPacketHeader = headerChunk;
			_currentInfoPacketChunks.Add(infoPacket);

			if (_currentInfoPacketChunks.Count == _lastInfoPacketHeader.FramePacketCount)
			{
				OnCompleteInfoFrameReceived(_lastInfoPacketHeader, systemNameChunk.SystemName, _currentInfoPacketChunks);
				_currentInfoPacketChunks.Clear();
			}
		}

		/// <summary>
		///     Override to provide new behavior for receipt of a complete PosiStageNet info frame
		/// </summary>
		/// <param name="header"></param>
		/// <param name="systemName"></param>
		/// <param name="infoPackets"></param>
		protected virtual void OnCompleteInfoFrameReceived(PsnInfoHeaderChunk header, string systemName,
			IReadOnlyCollection<PsnInfoPacketChunk> infoPackets)
		{
			var infoTrackerChunks = infoPackets.SelectMany(p =>
				((PsnInfoTrackerListChunk)p.SubChunks.Single(c => c.ChunkId == PsnInfoPacketChunkId.PsnInfoTrackerList))
					.SubChunks)
				.ToList();

			if (infoTrackerChunks.Select(c => c.TrackerId).Distinct().Count() != infoTrackerChunks.Count)
			{
				InvalidPacketReceived?.Invoke(this,
					new InvalidPacketsReceivedEventArgs(infoPackets, false, "Duplicate tracker IDs in frame"));
				return;
			}

			if (RemoteSystemName != systemName)
			{
				RemoteSystemName = systemName;
				RemoteSystemNameUpdated?.Invoke(this, RemoteSystemName);
			}

			foreach (var chunk in infoTrackerChunks)
			{
				PsnInfoTrackerNameChunk trackerNameChunk = null;

				foreach (var subChunk in chunk.SubChunks)
				{
					switch (subChunk.ChunkId)
					{
						case PsnInfoTrackerChunkId.PsnInfoTrackerName:

							if (trackerNameChunk != null)
							{
								InvalidPacketReceived?.Invoke(this,
									new InvalidPacketsReceivedEventArgs(infoPackets, !IsStrict,
										$"Tracker ID {chunk.TrackerId} has multiple tracker name chunks"));

								if (IsStrict)
									return;
							}

							trackerNameChunk = (PsnInfoTrackerNameChunk)subChunk;

							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				if (trackerNameChunk == null)
				{
					InvalidPacketReceived?.Invoke(this,
						new InvalidPacketsReceivedEventArgs(infoPackets, !IsStrict,
							$"Tracker ID {chunk.TrackerId} has no tracker name chunk"));

					if (IsStrict)
						return;

					continue;
				}

				if (!_trackers.TryGetValue(chunk.TrackerId, out var tracker))
				{
					tracker = new PsnTracker(chunk.TrackerId, trackerNameChunk.TrackerName, null, header.TimeStamp);
					_trackers.TryAdd(chunk.TrackerId, tracker);
				}
				else
				{
					tracker = tracker.WithTrackerName(trackerNameChunk.TrackerName);
					tracker = tracker.WithInfoTimeStamp(header.TimeStamp);
				}

				_trackers[chunk.TrackerId] = tracker;
			}

			TrackersUpdated?.Invoke(this, Trackers);
		}

		/// <summary>
		///     Override to provide new behavior for receipt of a PosiStageNet data packet
		/// </summary>
		protected virtual void OnDataPacketReceived(PsnDataPacketChunk dataPacket)
		{
			if (dataPacket.SubChunks.Count(c => c.ChunkId == PsnDataPacketChunkId.PsnDataHeader) > 1)
			{
				InvalidPacketReceived?.Invoke(this,
					new InvalidPacketsReceivedEventArgs(dataPacket, false,
						"Packet contains multiple data packet header chunks"));
				return;
			}

			var headerChunk =
				(PsnDataHeaderChunk)
					dataPacket.SubChunks.FirstOrDefault(c => c.ChunkId == PsnDataPacketChunkId.PsnDataHeader);

			if (headerChunk == null)
			{
				InvalidPacketReceived?.Invoke(this,
					new InvalidPacketsReceivedEventArgs(dataPacket, false, "Packet missing data packet header chunk"));
				return;
			}

			if (_currentDataPacketChunks.Any())
			{
				if (headerChunk.TimeStamp != _lastDataPacketHeader.TimeStamp
					|| headerChunk.FramePacketCount != _lastDataPacketHeader.FramePacketCount)
				{
					InvalidPacketReceived?.Invoke(this,
						new InvalidPacketsReceivedEventArgs(_currentDataPacketChunks, false,
							"Incomplete packet chunk discarded, did not receive all packets for data frame"));

					_currentDataPacketChunks.Clear();
				}
			}

			_lastDataPacketHeader = headerChunk;
			_currentDataPacketChunks.Add(dataPacket);

			if (_currentDataPacketChunks.Count == _lastDataPacketHeader.FramePacketCount)
			{
				OnCompleteDataFrameReceived(_lastDataPacketHeader, _currentDataPacketChunks);
				_currentDataPacketChunks.Clear();
			}
		}

		/// <summary>
		///     Override to provide new behavior for a receipt of a complete PosiStageNet data frame
		/// </summary>
		/// <param name="header"></param>
		/// <param name="dataPackets"></param>
		protected virtual void OnCompleteDataFrameReceived(PsnDataHeaderChunk header,
			IReadOnlyCollection<PsnDataPacketChunk> dataPackets)
		{
			var dataTrackerChunks = dataPackets.SelectMany(p =>
				((PsnDataTrackerListChunk)p.SubChunks.Single(c => c.ChunkId == PsnDataPacketChunkId.PsnDataTrackerList))
					.SubChunks)
				.ToList();

			if (dataTrackerChunks.Select(c => c.TrackerId).Distinct().Count() != dataTrackerChunks.Count)
			{
				InvalidPacketReceived?.Invoke(this,
					new InvalidPacketsReceivedEventArgs(dataPackets, false, "Duplicate tracker IDs in frame"));
				return;
			}

			foreach (var chunk in dataTrackerChunks)
			{
				if (!_trackers.TryGetValue(chunk.TrackerId, out var tracker))
					tracker = new PsnTracker(chunk.TrackerId);

				Tuple<float, float, float> position = null;
				Tuple<float, float, float> speed = null;
				Tuple<float, float, float> orientation = null;
				Tuple<float, float, float> acceleration = null;
				Tuple<float, float, float> targetPosition = null;
				ulong? timestamp = null;
				float? validity = null;

				foreach (var subChunk in chunk.SubChunks)
				{
					switch (subChunk.ChunkId)
					{
						case PsnDataTrackerChunkId.PsnDataTrackerPos:
							position = ((PsnDataTrackerPosChunk)subChunk).Vector;
							break;
						case PsnDataTrackerChunkId.PsnDataTrackerSpeed:
							speed = ((PsnDataTrackerSpeedChunk)subChunk).Vector;
							break;
						case PsnDataTrackerChunkId.PsnDataTrackerOri:
							orientation = ((PsnDataTrackerOriChunk)subChunk).Vector;
							break;
						case PsnDataTrackerChunkId.PsnDataTrackerStatus:
							validity = ((PsnDataTrackerStatusChunk)subChunk).Validity;
							break;
						case PsnDataTrackerChunkId.PsnDataTrackerAccel:
							acceleration = ((PsnDataTrackerAccelChunk)subChunk).Vector;
							break;
						case PsnDataTrackerChunkId.PsnDataTrackerTrgtPos:
							targetPosition = ((PsnDataTrackerTrgtPosChunk)subChunk).Vector;
							break;
                        case PsnDataTrackerChunkId.PsnDataTrackerTimestamp:
                            timestamp = ((PsnDataTrackerTimestampChunk)subChunk).Timestamp;
                            break;
						default:
							throw new ArgumentOutOfRangeException(nameof(subChunk.ChunkId));
					}
				}

				tracker = PsnTracker.CloneInternal(tracker, dataLastReceived: header.TimeStamp,
					position: position, clearPosition: position == null,
					speed: speed, clearSpeed: speed == null,
					orientation: orientation, clearOrientation: orientation == null,
					acceleration: acceleration, clearAcceleration: acceleration == null,
					targetPosition: targetPosition, clearTargetPosition: targetPosition == null,
					timestamp: timestamp, clearTimestamp: timestamp == null,
					validity: validity, clearValidity: validity == null);

				_trackers[chunk.TrackerId] = tracker;
			}

			TrackersUpdated?.Invoke(this, Trackers);
		}

		/// <summary>
		///     Override to provide new behavior for receipt of an unknown PosiStageNet packet type
		/// </summary>
		protected virtual void OnUnknownPacketReceived(PsnUnknownPacketChunk unknownPacket) { }



		private void packetReceived(object sender, UdpReceiveResult receiveResult)
        {
            if (receiveResult.Buffer == null)
                return;

			var chunk = PsnPacketChunk.FromByteArray(receiveResult.Buffer);

			if (chunk == null)
				return;

			OnPacketReceived(chunk);
		}

	    [CanBeNull]
	    private TChunk getSingleChunk<TChunk, TPacketChunk>(TPacketChunk packet, ushort chunkId, string packetType,
			string chunkType, bool isAllowMultiple = false, bool isMandatory = true) where TChunk : PsnChunk
			where TPacketChunk : PsnPacketChunk
		{
			if (packet.RawSubChunks.Count(c => c.RawChunkId == chunkId) > 1)
			{
				InvalidPacketReceived?.Invoke(this,
					new InvalidPacketsReceivedEventArgs(packet, isAllowMultiple,
						$"{packetType} packet contains multiple {chunkType} chunks"));

				if (!isAllowMultiple)
					return null;
			}

			var chunk = (TChunk)packet.RawSubChunks.FirstOrDefault(c => c.RawChunkId == chunkId);

			if (chunk == null)
			{
				InvalidPacketReceived?.Invoke(this,
					new InvalidPacketsReceivedEventArgs(packet, !isMandatory, $"{packetType} missing {chunkType} chunk"));
				return null;
			}

			return chunk;
		}



		/// <summary>
		///     Contains information on one of more invalid packets received by <see cref="PsnClient" />
		/// </summary>
		public class InvalidPacketsReceivedEventArgs : EventArgs
		{
			internal InvalidPacketsReceivedEventArgs(PsnPacketChunk packet, bool wasProcessed, string message)
			{
				Packets = new[] {packet};
				WasProcessed = wasProcessed;
				Message = message;
			}

			internal InvalidPacketsReceivedEventArgs(IEnumerable<PsnPacketChunk> packets, bool wasProcessed,
				string message)
			{
				Packets = packets.ToList();
				WasProcessed = wasProcessed;
				Message = message;
			}

			/// <summary>
			///     Invalid packet chunks
			/// </summary>
			public IEnumerable<PsnPacketChunk> Packets { get; }

			/// <summary>
			///     Description of the error
			/// </summary>
			public string Message { get; }

			/// <summary>
			///     Indicates if the invalid packets were processed by the client despite invalidity
			/// </summary>
			public bool WasProcessed { get; }

			/// <summary>
			///     Description of the error and contained packets
			/// </summary>
			public override string ToString()
				=> $"{nameof(InvalidPacketsReceivedEventArgs)}: {Message}, {string.Join(" ", Packets)}";
		}
	}
}