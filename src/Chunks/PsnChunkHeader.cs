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

namespace Imp.PosiStageDotNet.Chunks
{
	internal struct PsnChunkHeader : IEquatable<PsnChunkHeader>
	{
		public static PsnChunkHeader FromUInt32(uint value)
		{
			return new PsnChunkHeader((ushort)(value & 0x0000FFFF), (int)((value & 0x7FFF0000) >> 16),
				(value & 0x80000000) == 0x80000000);
		}

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

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is PsnChunkHeader && Equals((PsnChunkHeader)obj);
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
}