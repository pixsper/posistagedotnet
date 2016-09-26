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
	///     Abstract class representing a chunk which is the root of a PosiStageNet packet
	/// </summary>
	[PublicAPI]
	public abstract class PsnPacketChunk : PsnChunk
	{
		/// <summary>
		///		Base constructor for packet chunk
		/// </summary>
		/// <param name="subChunks">Untyped sub-chunks of this chunk</param>
		protected PsnPacketChunk([CanBeNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

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
		[CanBeNull]
		public static PsnPacketChunk FromByteArray(byte[] data)
		{
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
	[PublicAPI]
	public sealed class PsnUnknownPacketChunk : PsnPacketChunk, IEquatable<PsnUnknownPacketChunk>
	{
		internal PsnUnknownPacketChunk(ushort rawChunkId, [CanBeNull] byte[] data)
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
		public bool Equals([CanBeNull] PsnUnknownPacketChunk other)
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
		public override bool Equals([CanBeNull] object obj)
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
}