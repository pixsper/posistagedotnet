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
using DBDesign.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace DBDesign.PosiStageDotNet.Chunks
{
	/// <summary>
	///     Root chunk of a PosiStageNet info packet
	/// </summary>
	[PublicAPI]
	public sealed class PsnInfoPacketChunk : PsnPacketChunk
	{
		/// <summary>
		///		Info packet chunk constructor
		/// </summary>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnInfoPacketChunk([NotNull] IEnumerable<PsnInfoPacketSubChunk> subChunks)
			: this((IEnumerable<PsnChunk>)subChunks) { }

		/// <summary>
		///		Info packet chunk constructor
		/// </summary>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnInfoPacketChunk(params PsnInfoPacketSubChunk[] subChunks) : this((IEnumerable<PsnChunk>)subChunks) { }

		private PsnInfoPacketChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		/// <summary>
		///     The length of the data contained within this chunk, excluding sub-chunks and the local chunk header.
		/// </summary>
		public override int DataLength => 0;

		/// <summary>
		///		Typed chunk id for this packet chunk
		/// </summary>
		public override PsnPacketChunkId ChunkId => PsnPacketChunkId.PsnInfoPacket;

		/// <summary>
		///		Enumerable of typed sub-chunks contained within this chunk
		/// </summary>
		public IEnumerable<PsnInfoPacketSubChunk> SubChunks => RawSubChunks.OfType<PsnInfoPacketSubChunk>();

		/// <summary>
		///		Converts chunk and sub-chunks to an XML representation
		/// </summary>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnInfoPacketChunk),
				RawSubChunks.Select(c => c.ToXml()));
		}

		internal static PsnInfoPacketChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);

				switch ((PsnInfoPacketChunkId)pair.Item1.ChunkId)
				{
					case PsnInfoPacketChunkId.PsnInfoHeader:
						subChunks.Add(PsnInfoHeaderChunk.Deserialize(pair.Item1, reader));
						break;
					case PsnInfoPacketChunkId.PsnInfoSystemName:
						subChunks.Add(PsnInfoSystemNameChunk.Deserialize(pair.Item1, reader));
						break;
					case PsnInfoPacketChunkId.PsnInfoTrackerList:
						subChunks.Add(PsnInfoTrackerListChunk.Deserialize(pair.Item1, reader));
						break;
					default:
						subChunks.Add(PsnUnknownChunk.Deserialize(pair.Item1, reader));
						break;
				}
			}

			return new PsnInfoPacketChunk(subChunks);
		}
	}



	/// <summary>
	///		Base class for sub-chunks of a PosiStageNet info packet chunk
	/// </summary>
	[PublicAPI]
	public abstract class PsnInfoPacketSubChunk : PsnChunk
	{
		/// <summary>
		///		Base constructor for info packet sub-chunk
		/// </summary>
		/// <param name="subChunks"></param>
		protected PsnInfoPacketSubChunk([CanBeNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		/// <summary>
		///		Typed chunk ID for data packet sub-chunk
		/// </summary>
		public abstract PsnInfoPacketChunkId ChunkId { get; }

		/// <inheritdoc/>
		public override ushort RawChunkId => (ushort)ChunkId;
	}


	/// <summary>
	///		Chunk containing header data for a PosiStageNet info packet
	/// </summary>
	[PublicAPI]
	public sealed class PsnInfoHeaderChunk : PsnInfoPacketSubChunk, IEquatable<PsnInfoHeaderChunk>
	{
		internal const int StaticChunkAndHeaderLength = ChunkHeaderLength + StaticDataLength;
		internal const int StaticDataLength = 12;

		/// <summary>
		///		Constructs a header chunk for a info packet
		/// </summary>
		/// <param name="timestamp">Time in microseconds at which the info contained in this packet is valid</param>
		/// <param name="versionHigh">High byte of PosiStageNet version</param>
		/// <param name="versionLow">Low byte of PosiStageNet version</param>
		/// <param name="frameId">Frame ID byte</param>
		/// <param name="framePacketCount">Frame packet count byte</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public PsnInfoHeaderChunk(ulong timestamp, int versionHigh, int versionLow, int frameId, int framePacketCount)
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
		///		Time in microseconds at which the info contained in this packet is valid
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
		///		Frame ID value used for collating info from multiple packets
		/// </summary>
		public int FrameId { get; }

		/// <summary>
		///		Number of individual info packets for this <see cref="FrameId"/>
		/// </summary>
		public int FramePacketCount { get; }

		/// <inheritdoc/>
		public override int DataLength => StaticDataLength;

		/// <inheritdoc/>
		public override PsnInfoPacketChunkId ChunkId => PsnInfoPacketChunkId.PsnInfoHeader;

		/// <inheritdoc/>
		public bool Equals(PsnInfoHeaderChunk other)
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
			return new XElement(nameof(PsnInfoHeaderChunk),
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
			return obj.GetType() == GetType() && Equals((PsnInfoHeaderChunk)obj);
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

		internal static PsnInfoHeaderChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			ulong timeStamp = reader.ReadUInt64();
			int versionHigh = reader.ReadByte();
			int versionLow = reader.ReadByte();
			int frameId = reader.ReadByte();
			int framePacketCount = reader.ReadByte();

			return new PsnInfoHeaderChunk(timeStamp, versionHigh, versionLow, frameId, framePacketCount);
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