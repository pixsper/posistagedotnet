﻿// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace Pixsper.PosiStageDotNet.Chunks;

internal readonly struct PsnChunkHeader : IEquatable<PsnChunkHeader>
{
	public static PsnChunkHeader FromUInt32(uint value)
	{
		return new PsnChunkHeader((ushort)(value & 0x0000FFFF), (int)((value & 0x7FFF0000) >> 16),
			(value & 0x80000000) == 0x80000000);
	}

	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public PsnChunkHeader(ushort chunkId, int dataLength, bool hasSubChunks)
	{
		if (dataLength < ushort.MinValue || dataLength > ushort.MaxValue << 1)
			throw new ArgumentOutOfRangeException(nameof(dataLength), dataLength,
				$"Data length must be in range {ushort.MinValue}-{ushort.MaxValue << 1}");

		ChunkId = chunkId;
		DataLength = dataLength;
		HasSubChunks = hasSubChunks;
	}

	public ushort ChunkId { get; }
	public int DataLength { get; }
	public bool HasSubChunks { get; }

	public uint ToUInt32() => (uint)(ChunkId + (DataLength << 16) + (HasSubChunks ? 1 << 31 : 0));

	public bool Equals(PsnChunkHeader other)
	{
		return ChunkId == other.ChunkId && DataLength == other.DataLength && HasSubChunks == other.HasSubChunks;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		return obj is PsnChunkHeader header && Equals(header);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = ChunkId.GetHashCode();
			hashCode = (hashCode * 397) ^ DataLength;
			hashCode = (hashCode * 397) ^ HasSubChunks.GetHashCode();
			return hashCode;
		}
	}

	public override string ToString()
	{
		return $"Chunk ID: {ChunkId}, Data length: {DataLength}, Has Sub-Chunks: {HasSubChunks}";
	}

	public static bool operator ==(PsnChunkHeader left, PsnChunkHeader right) => left.Equals(right);

	public static bool operator !=(PsnChunkHeader left, PsnChunkHeader right) => !left.Equals(right);
}