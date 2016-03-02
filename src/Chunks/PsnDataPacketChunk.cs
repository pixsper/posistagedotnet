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
using System.IO;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	[PublicAPI]
	public class PsnDataPacketChunk : PsnChunk
	{
		internal static PsnDataPacketChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);

				switch ((PsnDataChunkId)pair.Item1.ChunkId)
				{
					case PsnDataChunkId.PsnDataPacketHeader:
						subChunks.Add(PsnDataPacketHeaderChunk.Deserialize(pair.Item1, reader));
						break;
					case PsnDataChunkId.PsnDataTrackerList:
						subChunks.Add(PsnDataTrackerListChunk.Deserialize(pair.Item1, reader));
						break;
					default:
						subChunks.Add(PsnUnknownChunk.Deserialize(pair.Item1, reader));
						break;
				}
			}

			return new PsnDataPacketChunk(subChunks);
		}

		public PsnDataPacketChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public PsnDataPacketChunk(params PsnChunk[] subChunks) : base(subChunks) { }

		public override ushort ChunkId => (ushort)PsnPacketChunkId.PsnDataPacket;
		public override int DataLength => 0;
	}


	[PublicAPI]
	public class PsnDataPacketHeaderChunk : PsnChunk, IEquatable<PsnDataPacketHeaderChunk>
	{
		internal static PsnDataPacketHeaderChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			ulong timeStamp = reader.ReadUInt64();
			int versionHigh = reader.ReadByte();
			int versionLow = reader.ReadByte();
			int frameId = reader.ReadByte();
			int framePacketCount = reader.ReadByte();

			return new PsnDataPacketHeaderChunk(timeStamp, versionHigh, versionLow, frameId, framePacketCount);
		}

		public PsnDataPacketHeaderChunk(ulong timestamp, int versionHigh, int versionLow, int frameId, int framePacketCount)
			: base(null)
		{
			TimeStamp = timestamp;

			if (versionHigh < 0 || versionHigh > 255)
				throw new ArgumentOutOfRangeException(nameof(versionHigh), "versionHigh must be between 0 and 255");

			VersionHigh = versionHigh;

			if (versionLow < 0 || versionLow > 255)
				throw new ArgumentOutOfRangeException(nameof(versionLow), "versionLow must be between 0 and 255");

			VersionLow = versionLow;

			if (frameId < 0 || frameId > 255)
				throw new ArgumentOutOfRangeException(nameof(frameId), "frameId must be between 0 and 255");

			FrameId = frameId;

			if (framePacketCount < 0 || framePacketCount > 255)
				throw new ArgumentOutOfRangeException(nameof(framePacketCount), "framePacketCount must be between 0 and 255");

			FramePacketCount = framePacketCount;
		}

		public ulong TimeStamp { get; }

		public int VersionHigh { get; }
		public int VersionLow { get; }
		public int FrameId { get; }
		public int FramePacketCount { get; }

		public override ushort ChunkId => (ushort)PsnDataChunkId.PsnDataPacketHeader;
		public override int DataLength => 12;

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(TimeStamp);
			writer.Write((byte)VersionHigh);
			writer.Write((byte)VersionLow);
			writer.Write((byte)FrameId);
			writer.Write((byte)FramePacketCount);
		}

		public bool Equals([CanBeNull] PsnDataPacketHeaderChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && TimeStamp == other.TimeStamp && VersionHigh == other.VersionHigh && VersionLow == other.VersionLow && FrameId == other.FrameId && FramePacketCount == other.FramePacketCount;
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == this.GetType() && Equals((PsnDataPacketHeaderChunk)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ TimeStamp.GetHashCode();
				hashCode = (hashCode * 397) ^ VersionHigh;
				hashCode = (hashCode * 397) ^ VersionLow;
				hashCode = (hashCode * 397) ^ FrameId;
				hashCode = (hashCode * 397) ^ FramePacketCount;
				return hashCode;
			}
		}
	}
}