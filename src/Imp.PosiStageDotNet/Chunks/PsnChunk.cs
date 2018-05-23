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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	/// <summary>
	///     Abstract class representing a chunk of PosiStageNet data. A chunk can either contain data or a collection of
	///     sub-chunks.
	/// </summary>
	[PublicAPI]
	public abstract class PsnChunk
	{
		/// <summary>
		///     Length in bytes of a PosiStageNet chunk header
		/// </summary>
		public const int ChunkHeaderLength = 4;

		/// <summary>
		///     Constructs a PosiStageNet chunk
		/// </summary>
		/// <param name="subChunks">Sub-chunks belonging to this chunk</param>
		protected PsnChunk([CanBeNull] IEnumerable<PsnChunk> subChunks)
		{
			RawSubChunks = subChunks ?? Enumerable.Empty<PsnChunk>();
		}

		/// <summary>
		///     The 16-bit identifier for this chunk.
		/// </summary>
		/// <remarks>Subclasses of PsnChunk have strongly typed ChunkId properties</remarks>
		public abstract ushort RawChunkId { get; }

		/// <summary>
		///     The length of the data contained within this chunk, excluding sub-chunks and the local chunk header.
		/// </summary>
		public abstract int DataLength { get; }

		/// <summary>
		///     The length of the entire chunk, including sub-chunks but excluding the local chunk header.
		/// </summary>
		public int ChunkLength => DataLength + RawSubChunks.Sum(c => ChunkHeaderLength + c.ChunkLength);

		/// <summary>
		///     The length of the entire chunk and local chunk header, including sub-chunks
		/// </summary>
		public int ChunkAndHeaderLength => ChunkLength + ChunkHeaderLength;


		/// <summary>
		///     Enumerable of sub-chunks
		/// </summary>
		public IEnumerable<PsnChunk> RawSubChunks { get; }

		/// <summary>
		///     True if this chunk contains sub-chunks
		/// </summary>
		public bool HasSubChunks => RawSubChunks.Any();

		/// <summary>
		///     Enumerable of sub-chunks which were unrecognized when deserializing
		/// </summary>
		public IEnumerable<PsnUnknownChunk> UnknownSubChunks => RawSubChunks.OfType<PsnUnknownChunk>();

		/// <summary>
		///     True if chunk contains any sub-chunks which were unrecognized when deserializing
		/// </summary>
		public bool HasUnknownSubChunks => UnknownSubChunks.Any();

		/// <summary>
		///     Chunk header value for this chunk
		/// </summary>
		internal PsnChunkHeader ChunkHeader => new PsnChunkHeader(RawChunkId, ChunkLength, HasSubChunks);

		/// <summary>
		///     Converts the chunk and sub-chunks to an XML representation
		/// </summary>
		public abstract XElement ToXml();

		/// <summary>
		///     Compares the chunk ID and sub-chunks of this chunk
		/// </summary>
		/// <param name="obj">Chunk to compare to this chunk</param>
		/// <returns>True if chunks are equal</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnChunk)obj);
		}

		/// <summary>
		///     Hashcode for this chunk based on chunk ID and sub-chunk enumerable
		/// </summary>
		public override int GetHashCode()
		{
			unchecked
			{
				return (RawChunkId.GetHashCode() * 397) ^ RawSubChunks.GetHashCode();
			}
		}

		/// <summary>
		///     Compares the chunk ID and sub-chunks of this chunk
		/// </summary>
		/// <param name="other">Chunk to compare to this chunk</param>
		/// <returns>True if chunks are equal</returns>
		protected bool Equals([CanBeNull] PsnChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return RawChunkId == other.RawChunkId && RawSubChunks.SequenceEqual(other.RawSubChunks);
		}

		/// <summary>
		///     Searches the binary stream for the positions of sub-chunk headers
		/// </summary>
		/// <exception cref="IOException">An I/O error occurs. </exception>
		/// <exception cref="NotSupportedException">The stream does not support seeking. </exception>
		internal static IEnumerable<Tuple<PsnChunkHeader, long>> FindSubChunkHeaders(PsnBinaryReader reader,
			int chunkDataLength)
		{
			var chunkHeaders = new List<Tuple<PsnChunkHeader, long>>();
			long startPos = reader.BaseStream.Position;

			while (reader.BaseStream.Position - startPos < chunkDataLength)
			{
				var chunkHeader = reader.ReadChunkHeader();
				chunkHeaders.Add(Tuple.Create(chunkHeader, reader.BaseStream.Position));
				reader.Seek(chunkHeader.DataLength, SeekOrigin.Current);

				Debug.Assert(reader.BaseStream.Position - startPos <= chunkDataLength);
			}

			reader.Seek(startPos, SeekOrigin.Begin);

			return chunkHeaders;
		}

		/// <summary>
		///     Serializes any data contained within this chunk, or nothing if the chunk contains no data
		/// </summary>
		/// <param name="writer"></param>
		internal virtual void SerializeData(PsnBinaryWriter writer) { }
	}



	/// <summary>
	///     Represents a PosiStageNet chunk which was unable to be deserialized as it's type is unknown
	/// </summary>
	[PublicAPI]
	public sealed class PsnUnknownChunk : PsnChunk, IEquatable<PsnUnknownChunk>
	{
		internal PsnUnknownChunk(ushort rawChunkId, [CanBeNull] byte[] data)
			: base(null)
		{
			RawChunkId = rawChunkId;
			Data = data ?? new byte[0];
		}

		/// <summary>
		///     Unserialized data contained in this chunk
		/// </summary>
		public byte[] Data { get; }

		/// <inheritdoc/>
		public override ushort RawChunkId { get; }

		/// <inheritdoc/>
		public override int DataLength => 0;


		/// <inheritdoc/>
		public bool Equals(PsnUnknownChunk other)
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
			return new XElement(nameof(PsnUnknownChunk),
				new XAttribute("DataLength", Data.Length));
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnUnknownChunk)obj);
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

		internal static PsnUnknownChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			// We can't proceed to deserialize any chunks from this point so store the raw data including sub-chunks
			return new PsnUnknownChunk(chunkHeader.ChunkId, reader.ReadBytes(chunkHeader.DataLength));
		}
	}
}