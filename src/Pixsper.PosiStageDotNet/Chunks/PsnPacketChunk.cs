// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Pixsper.PosiStageDotNet.Serialization;

namespace Pixsper.PosiStageDotNet.Chunks;

/// <summary>
///     Abstract class representing a chunk which is the root of a PosiStageNet packet
/// </summary>
public abstract class PsnPacketChunk : PsnChunk
{
	/// <summary>
	///		Base constructor for packet chunk
	/// </summary>
	/// <param name="subChunks">Untyped sub-chunks of this chunk</param>
	protected PsnPacketChunk(IEnumerable<PsnChunk>? subChunks) 
		: base(subChunks) { }

	/// <summary>
	///		Typed chunk ID for this packet chunk
	/// </summary>
	public abstract PsnPacketChunkId ChunkId { get; }

	/// <inheritdoc/>
	public override ushort RawChunkId => (ushort)ChunkId;

	/// <summary>
	///		Deserializes a PosiStageNet packet from a byte array
	/// </summary>
	/// <param name="data">Byte array containing PSN data</param>
	/// <returns>Chunk serialized within data</returns>
	public static PsnPacketChunk? FromByteArray(byte[] data)
	{
		if (data.Length == 0)
			return null;

		try
		{
			using (var ms = new MemoryStream(data))
			using (var reader = new PsnBinaryReader(ms))
			{
				var chunkHeader = reader.ReadChunkHeader();

				switch ((PsnPacketChunkId)chunkHeader.ChunkId)
				{
					case PsnPacketChunkId.PsnDataPacket:
						return PsnDataPacketChunk.Deserialize(chunkHeader, reader);
					case PsnPacketChunkId.PsnInfoPacket:
						return PsnInfoPacketChunk.Deserialize(chunkHeader, reader);
					default:
						return PsnUnknownPacketChunk.Deserialize(chunkHeader, reader);
				}
			}
		}
		catch (EndOfStreamException)
		{
			// Received a bad packet
			return null;
		}
	}

	/// <summary>
	///     Serializes the chunk to a byte array
	/// </summary>
	public byte[] ToByteArray()
	{
		using (var ms = new MemoryStream(ChunkHeaderLength + ChunkLength))
		using (var writer = new PsnBinaryWriter(ms))
		{
			serializeChunks(writer, new[] {this});
			return ms.ToArray();
		}
	}

	private void serializeChunks(PsnBinaryWriter writer, IEnumerable<PsnChunk> chunks)
	{
		foreach (var chunk in chunks)
		{
			writer.Write(chunk.ChunkHeader);
			chunk.SerializeData(writer);
			serializeChunks(writer, chunk.RawSubChunks);
		}
	}
}



/// <summary>
///     Represents a PosiStageNet packet chunk which was unable to be deserialized as it's type is unknown
/// </summary>
	
public sealed class PsnUnknownPacketChunk : PsnPacketChunk, IEquatable<PsnUnknownPacketChunk>
{
	internal PsnUnknownPacketChunk(ushort rawChunkId, byte[]? data)
		: base(null)
	{
		RawChunkId = rawChunkId;
		Data = data ?? new byte[0];
	}

	/// <summary>
	///		Un-deserialized data within chunk. May contain sub-chunks.
	/// </summary>
	public byte[] Data { get; }

	/// <inheritdoc/>
	public override PsnPacketChunkId ChunkId => PsnPacketChunkId.UnknownPacket;

	/// <inheritdoc/>
	public override ushort RawChunkId { get; }

	/// <inheritdoc/>
	public override int DataLength => 0;

	/// <inheritdoc/>
	public bool Equals(PsnUnknownPacketChunk? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && Data.SequenceEqual(other.Data);
	}

	/// <inheritdoc/>
	public override XElement ToXml()
	{
		return new XElement(nameof(PsnUnknownPacketChunk),
			new XAttribute("DataLength", Data.Length));
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		return obj.GetType() == GetType() && Equals((PsnUnknownPacketChunk)obj);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = base.GetHashCode();
			hashCode = (hashCode * 397) ^ Data.GetHashCode();
			hashCode = (hashCode * 397) ^ RawChunkId.GetHashCode();
			return hashCode;
		}
	}

	internal static PsnUnknownPacketChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
	{
		// We can't proceed to deserialize any chunks from this point so store the raw data including sub-chunks
		return new PsnUnknownPacketChunk(chunkHeader.ChunkId, reader.ReadBytes(chunkHeader.DataLength));
	}
}