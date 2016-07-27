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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Imp.PosiStageDotNet.Chunks;
using Imp.PosiStageDotNet.Networking;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet
{
    /// <summary>
    ///     Class to serialize and repeatedly send PosiStageNet Data and Info packets at defined rates
    /// </summary>
    /// <example>
    ///     // Setup
    ///     var server = new PsnServer("ExampleServer");
    ///     server.SetTrackers(new PsnTracker(0, "TestTracker", Tuple.Create(0d, 0d, 0d));
    ///     await server.StartSendingAsync();
    ///     
    ///     // When finished
    ///     server.Dispose();
    /// </example>
    [PublicAPI]
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class PsnServer : IDisposable
    {
        /// <summary>
        ///     High byte of PosiStageNet version number
        /// </summary>
        public const int VersionHigh = 2;

        /// <summary>
        ///     Low byte of PosiStagenet version number
        /// </summary>
        public const int VersionLow = 1;

        /// <summary>
        ///     Maximum PosiStageNet packet length
        /// </summary>
        public const int MaxPacketLength = 1500;

        /// <summary>
        ///     Standard IP address used by PosiStageNet
        /// </summary>
        public static readonly IPAddress DefaultMulticastIp = IPAddress.Parse("236.10.10.10");

        /// <summary>
        ///     Standard port used by PosiStageNet
        /// </summary>
        public const int DefaultPort = 56565;

        /// <summary>
        ///     Default frequency in Hz at which server should send data packets
        /// </summary>
        public const double DefaultDataSendFrequency = 60d;

        /// <summary>
        ///     Default frequency in Hz at which server should send info packets
        /// </summary>
        public const double DefaultInfoSendFrequency = 1d;

	    private readonly UdpService _udpService;
	    private readonly IPEndPoint _targetEndPoint;

        private Timer _dataTimer;

        private byte _frameId;
        private Timer _infoTimer;

        private bool _isDisposed;
        private Stopwatch _timeStampReference = new Stopwatch();

        private IEnumerable<PsnTracker> _trackers;

	    /// <summary>
	    ///     Constructs with default multicast IP, port number and data/info packet send frequencies
	    /// </summary>
	    /// <param name="systemName">Name used to identify this server</param>
	    /// <param name="localIp"></param>
	    /// <exception cref="ArgumentNullException"></exception>
	    /// <exception cref="ArgumentException"></exception>
	    /// <exception cref="ArgumentOutOfRangeException"></exception>
	    public PsnServer([NotNull] string systemName, IPAddress localIp = null)
            : this(systemName, DefaultMulticastIp, DefaultPort, DefaultDataSendFrequency, DefaultInfoSendFrequency, localIp) { }

	    /// <summary>
	    ///     Constructs with default multicast IP and port number
	    /// </summary>
	    /// <param name="systemName">Name used to identify this server</param>
	    /// <param name="dataSendFrequency">Custom frequency in Hz at which data packets are sent</param>
	    /// <param name="infoSendFrequency">Custom frequency in Hz at which info packets are sent</param>
	    /// <param name="localIp"></param>
	    /// <exception cref="ArgumentNullException"></exception>
	    /// <exception cref="ArgumentException"></exception>
	    /// <exception cref="ArgumentOutOfRangeException"></exception>
	    public PsnServer([NotNull] string systemName, double dataSendFrequency, double infoSendFrequency, IPAddress localIp = null)
            : this(systemName, DefaultMulticastIp, DefaultPort, dataSendFrequency, infoSendFrequency, localIp) { }

	    /// <summary>
	    ///     Constructs with default data/info packet send frequencies
	    /// </summary>
	    /// <param name="systemName">Name used to identify this server</param>
	    /// <param name="customMulticastIp">Custom multicast IP to send data to</param>
	    /// <param name="customPort">Custom UDP port number to send data to</param>
	    /// <param name="localIp"></param>
	    /// <exception cref="ArgumentNullException"></exception>
	    /// <exception cref="ArgumentException"></exception>
	    /// <exception cref="ArgumentOutOfRangeException"></exception>
	    public PsnServer([NotNull] string systemName, [NotNull] IPAddress customMulticastIp, int customPort, IPAddress localIp = null)
            : this(systemName, customMulticastIp, customPort, DefaultDataSendFrequency, DefaultInfoSendFrequency, localIp) { }

	    /// <summary>
	    ///     Constructs with custom values for all parameters
	    /// </summary>
	    /// <param name="systemName">Name used to identify this server</param>
	    /// <param name="dataSendFrequency">Custom frequency in Hz at which data packets are sent</param>
	    /// <param name="infoSendFrequency">Custom frequency in Hz at which info packets are sent</param>
	    /// <param name="customMulticastIp">Custom multicast IP to send data to</param>
	    /// <param name="customPort">Custom UDP port number to send data to</param>
	    /// <param name="localIp"></param>
	    /// <exception cref="ArgumentNullException"></exception>
	    /// <exception cref="ArgumentException"></exception>
	    /// <exception cref="ArgumentOutOfRangeException"></exception>
	    public PsnServer([NotNull] string systemName, double dataSendFrequency, double infoSendFrequency,
            [NotNull] IPAddress customMulticastIp, int customPort, IPAddress localIp = null)
            : this(systemName, customMulticastIp, customPort, dataSendFrequency, infoSendFrequency, localIp) { }


        private PsnServer([NotNull] string systemName, [NotNull] IPAddress multicastIp, int port, double dataSendFrequency,
            double infoSendFrequency, [CanBeNull] IPAddress localIp)
        {
            if (systemName == null)
                throw new ArgumentNullException(nameof(systemName));
	        SystemName = systemName;

			if (multicastIp == null)
				throw new ArgumentNullException(nameof(multicastIp));
			if (!multicastIp.IsIPv4Multicast())
				throw new ArgumentException("Not a valid IPv4 multicast address", nameof(multicastIp));

            if (port < ushort.MinValue + 1 || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port), port,
                    $"customPort must be in range {ushort.MinValue + 1}-{ushort.MaxValue}");

			_targetEndPoint = new IPEndPoint(multicastIp, port);

            if (dataSendFrequency <= 0)
                throw new ArgumentOutOfRangeException(nameof(dataSendFrequency), dataSendFrequency,
                    "dataSendFrequency must be greater than 0");
            DataSendFrequency = dataSendFrequency;

            if (infoSendFrequency <= 0)
                throw new ArgumentOutOfRangeException(nameof(infoSendFrequency), infoSendFrequency,
                    "infoSendFrequency must be greater than 0");

            InfoSendFrequency = infoSendFrequency;

	        LocalIp = localIp ?? IPAddress.Any;

			_udpService = new UdpService(new IPEndPoint(LocalIp, 0));
        }

	    /// <summary>
        ///     System name used to identify this PosiStageNet server
        /// </summary>
        public string SystemName { get; }

	    /// <summary>
	    ///     The multicast IP address the server is sending packets to
	    /// </summary>
	    public IPAddress MulticastIp => _targetEndPoint.Address;

	    /// <summary>
	    ///     The UDP port the server is sending packets to
	    /// </summary>
	    public int Port => _targetEndPoint.Port;


        /// <summary>
        ///     The IP address of the network adapter in use by the server, or <see cref="IPAddress.Any"/> if no adapter is set
        /// </summary>
        [CanBeNull]
        public IPAddress LocalIp { get; }

        /// <summary>
        ///     The current state of the server
        /// </summary>
        public bool IsSending { get; private set; }

        /// <summary>
        ///     The current time stamp referenced from the time the server started sending data
        /// </summary>
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
        ///     Stops sending data and releases network resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Set the collection of trackers to send data and info packets for
        /// </summary>
        public void SetTrackers([CanBeNull] IEnumerable<PsnTracker> trackers) => _trackers = trackers;

        /// <summary>
        ///     Set the collection of trackers to send data and info packets for
        /// </summary>
        public void SetTrackers(params PsnTracker[] trackers) => _trackers = trackers;

        /// <summary>
        ///     Send a custom PsnPacketChunk
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void SendCustomPacket([NotNull] PsnPacketChunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            var bytes = chunk.ToByteArray();

            if (bytes.Length > MaxPacketLength)
                throw new ArgumentException(
                    $"Serialized chunk length ({bytes.Length}) is longer than maximum PosiStageNet packet length ({MaxPacketLength})");

            _udpService.SendAsync(bytes, _targetEndPoint).Wait();
        }

        /// <summary>
        ///     Joins the server to the multicast IP and starts sending PosiStageNet packets
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void StartSending()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PsnServer));

            IsSending = true;
            _timeStampReference.Restart();
            _dataTimer = new Timer(sendData, 0, (int)(1000d / DataSendFrequency));
            _infoTimer = new Timer(sendInfo, 0, (int)(1000d / InfoSendFrequency));
        }

        /// <summary>
        ///     Removes the server from the multicast group and stops sending PosiStageNet packets
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void StopSending()
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

            IsSending = false;
        }

        /// <summary>
        ///      Stops sending data and releases network resources
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
	            if (IsSending)
		            StopSending();

                _udpService.Dispose();
            }

            _isDisposed = true;
        }

        /// <summary>
        ///     Override to provide new behavior for transmission of PosiStageNet info packets
        /// </summary>
        /// <param name="trackers">Enumerable of trackers to send info packets for</param>
        protected virtual void OnSendInfo([NotNull] IEnumerable<PsnTracker> trackers)
        {
            var systemNameChunk = new PsnInfoSystemNameChunk(SystemName);
            var trackerChunks = trackers.Select(t => new PsnInfoTrackerChunk(t.TrackerId, t.ToInfoTrackerChunks()));

            var trackerListChunks = new List<PsnInfoTrackerListChunk>();
            var currentTrackerList = new List<PsnInfoTrackerChunk>();

            int trackerListLength = 0;
            int maxTrackerListLength = MaxPacketLength -
                                       (PsnChunk.ChunkHeaderLength // Root Chunk-Header
                                        + PsnInfoHeaderChunk.StaticChunkAndHeaderLength // Packet Header Chunk
                                        + systemNameChunk.ChunkAndHeaderLength // System Name Chunk
                                        + PsnChunk.ChunkHeaderLength); // Tracker List Chunk-Header

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

                _udpService.SendAsync(data, _targetEndPoint).Wait();
            }
        }

        /// <summary>
        ///     Override to provide new behavior for transmission of PosiStageNet data packets
        /// </summary>
        /// <param name="trackers">Enumerable of trackers to send data packets for</param>
        protected virtual void OnSendData([NotNull] IEnumerable<PsnTracker> trackers)
        {
            var trackerChunks = trackers.Select(t => new PsnDataTrackerChunk(t.TrackerId, t.ToDataTrackerChunks()));

            var trackerListChunks = new List<PsnDataTrackerListChunk>();
            var currentTrackerList = new List<PsnDataTrackerChunk>();

            int trackerListLength = 0;
            int maxTrackerListLength = MaxPacketLength -
                                       (PsnChunk.ChunkHeaderLength // Root Chunk-Header
                                        + PsnDataHeaderChunk.StaticChunkAndHeaderLength // Packet Header Chunk
                                        + PsnChunk.ChunkHeaderLength); // Tracker List Chunk-Header

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

                _udpService.SendAsync(data, _targetEndPoint).Wait();
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