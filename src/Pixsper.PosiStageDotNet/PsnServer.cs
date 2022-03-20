﻿// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Pixsper.PosiStageDotNet.Chunks;
using Pixsper.PosiStageDotNet.Networking;

namespace Pixsper.PosiStageDotNet;

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

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class PsnServer : IDisposable
{
	/// <summary>
	///     High byte of PosiStageNet version number
	/// </summary>
	public const int VersionHigh = 2;

	/// <summary>
	///     Low byte of PosiStageNet version number
	/// </summary>
	public const int VersionLow = 3;

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

	private Timer? _dataTimer;

	private byte _frameId;
	private Timer? _infoTimer;

	private bool _isDisposed;
	private Stopwatch _timeStampReference = new Stopwatch();

	private readonly ConcurrentDictionary<int, PsnTracker> _trackers = new ConcurrentDictionary<int, PsnTracker>();

	/// <summary>
	///     Constructs with default multicast IP, port number and data/info packet send frequencies
	/// </summary>
	/// <param name="systemName">Name used to identify this server</param>
	/// <param name="localIp">IP of local network adapter to listen for multicast packets using, or null to send on all network adapters</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public PsnServer(string systemName, IPAddress? localIp = null)
		: this(systemName, DefaultMulticastIp, DefaultPort, DefaultDataSendFrequency, DefaultInfoSendFrequency, localIp) { }

	/// <summary>
	///     Constructs with default multicast IP and port number
	/// </summary>
	/// <param name="systemName">Name used to identify this server</param>
	/// <param name="dataSendFrequency">Custom frequency in Hz at which data packets are sent upon calling <see cref="StartSending"/></param>
	/// <param name="infoSendFrequency">Custom frequency in Hz at which info packets are sent upon calling <see cref="StartSending"/></param>
	/// <param name="localIp">IP of local network adapter to listen for multicast packets using, or null to send on all network adapters</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public PsnServer(string systemName, double dataSendFrequency, double infoSendFrequency, IPAddress? localIp = null)
		: this(systemName, DefaultMulticastIp, DefaultPort, dataSendFrequency, infoSendFrequency, localIp) { }

	/// <summary>
	///     Constructs with default data/info packet send frequencies
	/// </summary>
	/// <param name="systemName">Name used to identify this server</param>
	/// <param name="customMulticastIp">Custom multicast IP to send data to</param>
	/// <param name="customPort">Custom UDP port number to send data to</param>
	/// <param name="localIp">IP of local network adapter to listen for multicast packets using, or null to send on all network adapters</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public PsnServer(string systemName, IPAddress customMulticastIp, int customPort, IPAddress? localIp = null)
		: this(systemName, customMulticastIp, customPort, DefaultDataSendFrequency, DefaultInfoSendFrequency, localIp) { }

	/// <summary>
	///     Constructs with custom values for all parameters
	/// </summary>
	/// <param name="systemName">Name used to identify this server</param>
	/// <param name="dataSendFrequency">Custom frequency in Hz at which data packets are sent</param>
	/// <param name="infoSendFrequency">Custom frequency in Hz at which info packets are sent</param>
	/// <param name="customMulticastIp">Custom multicast IP to send data to</param>
	/// <param name="customPort">Custom UDP port number to send data to</param>
	/// <param name="localIp">IP of local network adapter to listen for multicast packets using, or null to send on all network adapters</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public PsnServer(string systemName, double dataSendFrequency, double infoSendFrequency,
		IPAddress customMulticastIp, int customPort, IPAddress? localIp = null)
		: this(systemName, customMulticastIp, customPort, dataSendFrequency, infoSendFrequency, localIp) { }


	private PsnServer(string systemName, IPAddress multicastIp, int port, double dataSendFrequency,
		double infoSendFrequency, IPAddress? localIp)
	{
		SystemName = systemName ?? throw new ArgumentNullException(nameof(systemName));

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

		_timeStampReference.Start();
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
	public IPAddress? LocalIp { get; }

	/// <summary>
	///     The current state of the server
	/// </summary>
	public bool IsSendingAutomatic { get; private set; }

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
	///     Clears any existing trackers and adds new trackers from collection
	/// </summary>
	public void SetTrackers(IEnumerable<PsnTracker>? trackers)
	{
		_trackers.Clear();

		if (trackers == null)
			return;

		foreach (var t in trackers)
			_trackers[t.TrackerId] = t;
	}

	/// <summary>
	///     Clears any existing trackers and adds new trackers from collection
	/// </summary>
	public void SetTrackers(params PsnTracker[] trackers) => SetTrackers((IEnumerable<PsnTracker>)trackers);

	/// <summary>
	///		Updates values for existing trackers. If trackers do not exist they will be added
	/// </summary>
	public void UpdateTrackers(IEnumerable<PsnTracker> trackers)
	{
		foreach (var t in trackers)
			_trackers[t.TrackerId] = t;
	}

	/// <summary>
	///		Updates values for existing trackers. If trackers do not exist they will be added
	/// </summary>
	public void UpdateTrackers(params PsnTracker[] trackers) => UpdateTrackers((IEnumerable<PsnTracker>)trackers);

	/// <summary>
	///     Removes trackers with specified indexes. Return false if any trackers indexes were not found.
	/// </summary>
	public bool RemoveTrackers(IEnumerable<int> trackerIndexes)
	{
		bool isSuccess = true;

		foreach (var i in trackerIndexes)
		{
			if (!_trackers.TryRemove(i, out var value))
				isSuccess = false;
		}

		return isSuccess;
	}

	/// <summary>
	///     Removes trackers with specified indexes. Return false if any trackers indexes were not found.
	/// </summary>
	public bool RemoveTrackers(params int[] trackerIndexes) => RemoveTrackers((IEnumerable<int>)trackerIndexes);

	/// <summary>
	///		Removes all existing trackers
	/// </summary>
	public void RemoveAllTrackers()
	{
		_trackers.Clear();
	}


	/// <summary>
	///     Send a custom PsnPacketChunk
	/// </summary>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	public void SendCustomPacket(PsnPacketChunk chunk)
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
	///     Starts automatically sending PosiStageNet packets using <see cref="DataSendFrequency"/> and <see cref="InfoSendFrequency"/> rates.
	/// </summary>
	/// <exception cref="ObjectDisposedException"></exception>
	public void StartSending(bool isResetTimestampReference = false)
	{
		if (_isDisposed)
			throw new ObjectDisposedException(nameof(PsnServer));

		if (isResetTimestampReference)
			_timeStampReference.Restart();

		IsSendingAutomatic = true;
		_dataTimer = new Timer(sendData, 0, (int)(1000d / DataSendFrequency));
		_infoTimer = new Timer(sendInfo, 0, (int)(1000d / InfoSendFrequency));
	}

	/// <summary>
	///     Stops automatically sending PosiStageNet packets
	/// </summary>
	/// <exception cref="ObjectDisposedException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public void StopSending()
	{
		if (_isDisposed)
			throw new ObjectDisposedException(nameof(PsnServer));

		if (!IsSendingAutomatic)
			throw new InvalidOperationException("Cannot stop sending, client is not currently sending");

		_dataTimer?.Dispose();
		_dataTimer = null;
		_infoTimer?.Dispose();
		_infoTimer = null;

		IsSendingAutomatic = false;
	}

	/// <summary>
	///		Sends info data. Can only be used when <see cref="StartSending"/> has not been called and <see cref="IsSendingAutomatic"/> is false
	/// </summary>
	public void SendInfo()
	{
		if (IsSendingAutomatic)
			throw new InvalidOperationException("Cannot send info when in automatic sending is active.");

		sendInfo();
	}

	/// <summary>
	///		Sends info data. Can only be used when <see cref="StartSending"/> has not been called and <see cref="IsSendingAutomatic"/> is false
	/// </summary>
	public void SendData()
	{if (IsSendingAutomatic)
			throw new InvalidOperationException("Cannot send data when in automatic sending is active.");

		sendData();
	}

	/// <summary>
	///		Resets timestamp reference used for data packet output to zero
	/// </summary>
	public void ResetTimestampReference()
	{
		_timeStampReference.Restart();
	}

	/// <summary>
	///      Stops sending data and releases network resources
	/// </summary>
	/// <param name="isDisposing"></param>
	protected virtual void Dispose(bool isDisposing)
	{
		if (isDisposing)
		{
			if (IsSendingAutomatic)
				StopSending();

			_udpService.Dispose();
		}

		_isDisposed = true;
	}

	/// <summary>
	///     Override to provide new behavior for transmission of PosiStageNet info packets
	/// </summary>
	/// <param name="trackers">Enumerable of trackers to send info packets for</param>
	protected virtual void OnSendInfo(IEnumerable<PsnTracker> trackers)
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
	protected virtual void OnSendData(IEnumerable<PsnTracker> trackers)
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
		if (_trackers == null)
			return;

		OnSendInfo(_trackers.Values);
	}

	private void sendData()
	{
		if (_trackers == null)
			return;

		OnSendData(_trackers.Values);
	}



	internal class Timer : CancellationTokenSource, IDisposable
	{
		public Timer(Action callback, int dueTime, int period)
		{
			Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
				{
					var action = (Action)s!;

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