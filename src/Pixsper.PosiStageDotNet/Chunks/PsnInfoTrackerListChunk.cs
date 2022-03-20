﻿// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Pixsper.PosiStageDotNet.Serialization;

namespace Pixsper.PosiStageDotNet.Chunks;

/// <summary>
///     PosiStageNet chunk containing a list of info trackers
/// </summary>
public sealed class PsnInfoTrackerListChunk : PsnInfoPacketSubChunk
{
	/// <summary>
	///		Info tracker list chunk constructor
	/// </summary>
	/// <param name="subChunks">Typed sub-chunks of this chunk</param>
	public PsnInfoTrackerListChunk(IEnumerable<PsnInfoTrackerChunk> subChunks)
		: this((IEnumerable<PsnChunk>)subChunks)
	{ }

	/// <summary>
	///		Info tracker list chunk constructor
	/// </summary>
	/// <param name="subChunks">Typed sub-chunks of this chunk</param>
	public PsnInfoTrackerListChunk(params PsnInfoTrackerChunk[] subChunks) : this((IEnumerable<PsnChunk>)subChunks) { }

	private PsnInfoTrackerListChunk(IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

	/// <inheritdoc/>
	public override PsnInfoPacketChunkId ChunkId => PsnInfoPacketChunkId.PsnInfoTrackerList;

	/// <inheritdoc/>
	public override int DataLength => 0;

	/// <summary>
	///		Typed sub-chunks of this chunk
	/// </summary>
	public IEnumerable<PsnInfoTrackerChunk> SubChunks => RawSubChunks.OfType<PsnInfoTrackerChunk>();

	/// <inheritdoc/>
	public override XElement ToXml()
	{
		return new XElement(nameof(PsnInfoTrackerListChunk),
			RawSubChunks.Select(c => c.ToXml()));
	}

	internal static PsnInfoTrackerListChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
	{
		var subChunks = new List<PsnChunk>();

		foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
		{
			reader.Seek(pair.Item2, SeekOrigin.Begin);
			subChunks.Add(PsnInfoTrackerChunk.Deserialize(pair.Item1, reader));
		}

		return new PsnInfoTrackerListChunk(subChunks);
	}
}

/// <summary>
///		PosiStageNet info packet chunk containing info for one info tracker. Only valid as sub-chunk of a <see cref="PsnInfoTrackerListChunk"/>.
/// </summary>
	
public class PsnInfoTrackerChunk : PsnChunk
{
	/// <summary>
	///		Info tracker chunk constructor
	/// </summary>
	/// <param name="trackerId">ID of this tracker</param>
	/// <param name="subChunks">Typed sub-chunks of this chunk</param>
	public PsnInfoTrackerChunk(int trackerId, IEnumerable<PsnInfoTrackerSubChunk> subChunks)
		: this(trackerId, (IEnumerable<PsnChunk>)subChunks)
	{ }

	/// <summary>
	///		Info tracker chunk constructor
	/// </summary>
	/// /// <param name="trackerId">ID of this tracker</param>
	/// <param name="subChunks">Typed sub-chunks of this chunk</param>
	public PsnInfoTrackerChunk(int trackerId, params PsnInfoTrackerSubChunk[] subChunks)
		: this(trackerId, (IEnumerable<PsnChunk>)subChunks)
	{ }

	private PsnInfoTrackerChunk(int trackerId, IEnumerable<PsnChunk> subChunks)
		: base(subChunks)
	{
		if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
				$"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

		RawChunkId = (ushort)trackerId;
	}

	/// <summary><inheritdoc/></summary>
	/// <remarks>
	///		Trackers have no specific chunk ID and use this value to store the tracker ID
	/// </remarks>
	public override ushort RawChunkId { get; }

	/// <inheritdoc/>
	public override int DataLength => 0;

	/// <summary>
	///		ID of this tracker, stored in <see cref="RawChunkId"/>
	/// </summary>
	public int TrackerId => RawChunkId;

	/// <summary>
	///		Typed sub-chunks of this chunk
	/// </summary>
	public IEnumerable<PsnInfoTrackerSubChunk> SubChunks => RawSubChunks.OfType<PsnInfoTrackerSubChunk>();

	/// <inheritdoc/>
	public override XElement ToXml()
	{
		return new XElement(nameof(PsnInfoTrackerChunk),
			new XAttribute("TrackerId", RawChunkId),
			RawSubChunks.Select(c => c.ToXml()));
	}

	internal static PsnInfoTrackerChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
	{
		var subChunks = new List<PsnChunk>();

		foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
		{
			reader.Seek(pair.Item2, SeekOrigin.Begin);

			switch ((PsnInfoTrackerChunkId)pair.Item1.ChunkId)
			{
				case PsnInfoTrackerChunkId.PsnInfoTrackerName:
					subChunks.Add(PsnInfoTrackerNameChunk.Deserialize(pair.Item1, reader));
					break;
				default:
					subChunks.Add(PsnUnknownChunk.Deserialize(chunkHeader, reader));
					break;
			}
		}

		return new PsnInfoTrackerChunk(chunkHeader.ChunkId, subChunks);
	}
}



/// <summary>
///		Base class for sub-chunks of <see cref="PsnInfoTrackerChunk"/>
/// </summary>
	
public abstract class PsnInfoTrackerSubChunk : PsnChunk
{
	/// <summary>
	///		Base info tracker sub-chunk constructor
	/// </summary>
	/// <param name="subChunks">Typed sub-chunks of this chunk</param>
	protected PsnInfoTrackerSubChunk(IEnumerable<PsnChunk>? subChunks) : base(subChunks) { }

	/// <summary>
	///		Typed chunk ID
	/// </summary>
	public abstract PsnInfoTrackerChunkId ChunkId { get; }

	/// <inheritdoc/>
	public override ushort RawChunkId => (ushort)ChunkId;
}



/// <summary>
///		Info tracker sub-chunk containing tracker name
/// </summary>
	
public class PsnInfoTrackerNameChunk : PsnInfoTrackerSubChunk, IEquatable<PsnInfoTrackerNameChunk>
{
	/// <summary>
	///		Tracker name chunk constructor
	/// </summary>
	/// <param name="trackerName">Name for tracker with this tracker ID</param>
	/// <exception cref="ArgumentNullException"><paramref name="trackerName"/> is <see langword="null" />.</exception>
	public PsnInfoTrackerNameChunk(string trackerName)
		: base(null)
	{
		TrackerName = trackerName ?? throw new ArgumentNullException(nameof(trackerName));
	}

	/// <summary>
	///		Name for tracker with this tracker ID
	/// </summary>
	public string TrackerName { get; }

	/// <inheritdoc/>
	public override int DataLength => TrackerName.Length;

	/// <inheritdoc/>
	public override PsnInfoTrackerChunkId ChunkId => PsnInfoTrackerChunkId.PsnInfoTrackerName;

	/// <inheritdoc/>
	public bool Equals(PsnInfoTrackerNameChunk? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && string.Equals(TrackerName, other.TrackerName);
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		return obj.GetType() == GetType() && Equals((PsnInfoTrackerNameChunk)obj);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unchecked
		{
			return (base.GetHashCode() * 397) ^ TrackerName.GetHashCode();
		}
	}

	/// <inheritdoc/>
	public override XElement ToXml()
	{
		return new XElement(nameof(PsnInfoTrackerNameChunk),
			new XAttribute(nameof(TrackerName), TrackerName));
	}

	internal static PsnInfoTrackerNameChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
	{
		return new PsnInfoTrackerNameChunk(reader.ReadString(chunkHeader.DataLength));
	}

	internal override void SerializeData(PsnBinaryWriter writer)
	{
		writer.Write(TrackerName);
	}
}