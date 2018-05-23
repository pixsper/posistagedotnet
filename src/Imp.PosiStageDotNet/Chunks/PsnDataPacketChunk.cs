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
using System.Linq;
using System.Xml.Linq;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	/// <summary>
	///     Root chunk of a PosiStageNet data packet
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataPacketChunk : PsnPacketChunk
	{
		/// <summary>
		///		Data chunk constructor
		/// </summary>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnDataPacketChunk([NotNull] IEnumerable<PsnDataPacketSubChunk> subChunks)
			: this((IEnumerable<PsnChunk>)subChunks) { }

		/// <summary>
		///		Data chunk constructor
		/// </summary>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnDataPacketChunk(params PsnDataPacketSubChunk[] subChunks) : this((IEnumerable<PsnChunk>)subChunks) { }

		private PsnDataPacketChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		/// <inheritdoc/>
		public override int DataLength => 0;

		/// <summary>
		///		Enumerable of typed sub-chunks contained within this chunk
		/// </summary>
		public IEnumerable<PsnDataPacketSubChunk> SubChunks => RawSubChunks.OfType<PsnDataPacketSubChunk>();

		/// <inheritdoc/>
		public override PsnPacketChunkId ChunkId => PsnPacketChunkId.PsnDataPacket;

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataPacketChunk),
				RawSubChunks.Select(c => c.ToXml()));
		}

		internal static PsnDataPacketChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);

				switch ((PsnDataPacketChunkId)pair.Item1.ChunkId)
				{
					case PsnDataPacketChunkId.PsnDataHeader:
						subChunks.Add(PsnDataHeaderChunk.Deserialize(pair.Item1, reader));
						break;
					case PsnDataPacketChunkId.PsnDataTrackerList:
						subChunks.Add(PsnDataTrackerListChunk.Deserialize(pair.Item1, reader));
						break;
					default:
						subChunks.Add(PsnUnknownChunk.Deserialize(pair.Item1, reader));
						break;
				}
			}

			return new PsnDataPacketChunk(subChunks);
		}
	}


	/// <summary>
	///		Base class for sub-chunks of a PosiStageNet data packet chunk
	/// </summary>
	[PublicAPI]
	public abstract class PsnDataPacketSubChunk : PsnChunk
	{
		/// <summary>
		///		Base constructor for data packet sub-chunk
		/// </summary>
		/// <param name="subChunks"></param>
		protected PsnDataPacketSubChunk([CanBeNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		/// <summary>
		///		Typed chunk ID for data packet sub-chunk
		/// </summary>
		public abstract PsnDataPacketChunkId ChunkId { get; }

		/// <inheritdoc/>
		public override ushort RawChunkId => (ushort)ChunkId;
	}



	/// <summary>
	///		Chunk containing header data for a PosiStageNet data packet
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataHeaderChunk : PsnDataPacketSubChunk, IEquatable<PsnDataHeaderChunk>
	{
		internal const int StaticChunkAndHeaderLength = ChunkHeaderLength + StaticDataLength;
		internal const int StaticDataLength = 12;


		/// <summary>
		///		Constructs a header chunk for a data packet
		/// </summary>
		/// <param name="timestamp">Time in microseconds at which the data contained in this packet was measured</param>
		/// <param name="versionHigh">High byte of PosiStageNet version</param>
		/// <param name="versionLow">Low byte of PosiStageNet version</param>
		/// <param name="frameId">Frame ID byte</param>
		/// <param name="framePacketCount">Frame packet count byte</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public PsnDataHeaderChunk(ulong timestamp, int versionHigh, int versionLow, int frameId, int framePacketCount)
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
				throw new ArgumentOutOfRangeException(nameof(framePacketCount),
					"framePacketCount must be between 0 and 255");

			FramePacketCount = framePacketCount;
		}

		/// <summary>
		///		Time in microseconds at which the data contained in this packet was measured
		/// </summary>
		public ulong TimeStamp { get; }

		/// <summary>
		///		High byte of the PosiStageNet version of the packet
		/// </summary>
		public int VersionHigh { get; }

		/// <summary>
		///		Low byte of the PosiStageNet version of this packet
		/// </summary>
		public int VersionLow { get; }

		/// <summary>
		///		Frame ID value used for collating data from multiple packets
		/// </summary>
		public int FrameId { get; }

		/// <summary>
		///		Number of individual data packets for this <see cref="FrameId"/>
		/// </summary>
		public int FramePacketCount { get; }

		/// <inheritdoc/>
		public override int DataLength => StaticDataLength;

		/// <inheritdoc/>
		public override PsnDataPacketChunkId ChunkId => PsnDataPacketChunkId.PsnDataHeader;

		/// <inheritdoc/>
		public bool Equals(PsnDataHeaderChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && TimeStamp == other.TimeStamp && VersionHigh == other.VersionHigh
				   && VersionLow == other.VersionLow && FrameId == other.FrameId
				   && FramePacketCount == other.FramePacketCount;
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataHeaderChunk),
				new XAttribute(nameof(TimeStamp), TimeStamp),
				new XAttribute(nameof(VersionHigh), VersionHigh),
				new XAttribute(nameof(FrameId), FrameId),
				new XAttribute(nameof(FramePacketCount), FramePacketCount));
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataHeaderChunk)obj);
		}

		/// <inheritdoc/>
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

		internal static PsnDataHeaderChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			ulong timeStamp = reader.ReadUInt64();
			int versionHigh = reader.ReadByte();
			int versionLow = reader.ReadByte();
			int frameId = reader.ReadByte();
			int framePacketCount = reader.ReadByte();

			return new PsnDataHeaderChunk(timeStamp, versionHigh, versionLow, frameId, framePacketCount);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(TimeStamp);
			writer.Write((byte)VersionHigh);
			writer.Write((byte)VersionLow);
			writer.Write((byte)FrameId);
			writer.Write((byte)FramePacketCount);
		}
	}
}