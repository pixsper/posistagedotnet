using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Imp.PosiStageDotNet.Chunks;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet
{
	internal abstract class PsnChunk
	{
		[CanBeNull]
		public static PsnChunk FromByteArray(byte[] data)
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
							return PsnUnknownChunk.Deserialize(chunkHeader, reader);
					}
				}
			}
			catch (EndOfStreamException)
			{
				// Received a bad packet
				return null;
			}
		}

		protected static IEnumerable<Tuple<PsnChunkHeader, long>> FindSubChunkHeaders(PsnBinaryReader reader,
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

		public const int ChunkHeaderLength = 4;

		protected PsnChunk([CanBeNull] IEnumerable<PsnChunk> subChunks)
		{
			SubChunks = subChunks ?? Enumerable.Empty<PsnChunk>();
		}

		/// <summary>
		///     The 16-bit identifier for this chunk
		/// </summary>
		public abstract ushort ChunkId { get; }

		/// <summary>
		///     The length of the data contained within this chunk, excluding sub-chunks and the local chunk header.
		/// </summary>
		public abstract int DataLength { get; }

		/// <summary>
		///     The length of the entire chunk, including sub-chunks but excluding the local chunk header.
		/// </summary>
		public int ChunkLength => DataLength + SubChunks.Sum(c => ChunkHeaderLength + c.ChunkLength);


		/// <summary>
		///     Enumerable of sub-chunks
		/// </summary>
		public IEnumerable<PsnChunk> SubChunks { get; }

		/// <summary>
		///     True if this chunk contains sub-chunks
		/// </summary>
		public bool HasSubChunks => SubChunks.Any();

		/// <summary>
		///     Chunk header value for this chunk
		/// </summary>
		public PsnChunkHeader ChunkHeader => new PsnChunkHeader(ChunkId, ChunkLength, HasSubChunks);

		/// <summary>
		///     Serializes the chunk to a byte array
		/// </summary>
		public byte[] ToByteArray()
		{
			using (var ms = new MemoryStream(ChunkHeaderLength + ChunkLength))
			using (var writer = new PsnBinaryWriter(ms))
			{
				writer.Write(ChunkHeader);
				SerializeData(writer);
				serializeChunks(writer, SubChunks);

				return ms.ToArray();
			}
		}

		/// <summary>
		///     Serializes any data contained within this chunk, or nothing if the chunk contains no data
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void SerializeData(PsnBinaryWriter writer) { }

		protected bool Equals([CanBeNull] PsnChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return ChunkId == other.ChunkId && SubChunks.SequenceEqual(other.SubChunks);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == this.GetType() && Equals((PsnChunk)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (ChunkId.GetHashCode() * 397) ^ SubChunks.GetHashCode();
			}
		}

		private void serializeChunks(PsnBinaryWriter writer, IEnumerable<PsnChunk> chunks)
		{
			foreach (var chunk in chunks)
			{
				writer.Write(chunk.ChunkHeader);
				chunk.SerializeData(writer);
				serializeChunks(writer, chunk.SubChunks);
			}
		}
	}



	internal class PsnUnknownChunk : PsnChunk, IEquatable<PsnUnknownChunk>
	{
		public static PsnUnknownChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			// We can't proceed to deserialize any chunks from this point so store the raw data including sub-chunks
			return new PsnUnknownChunk(chunkHeader.ChunkId, reader.ReadBytes(chunkHeader.DataLength));
		}

		public PsnUnknownChunk(ushort chunkId, [CanBeNull] byte[] data)
			: base(null)
		{
			ChunkId = chunkId;
			Data = data ?? new byte[0];
		}

		public byte[] Data { get; }

		public override ushort ChunkId { get; }
		public override int DataLength => 0;

		public bool Equals([CanBeNull] PsnUnknownChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && Data.SequenceEqual(other.Data);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == this.GetType() && Equals((PsnUnknownChunk)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ Data.GetHashCode();
				hashCode = (hashCode * 397) ^ ChunkId.GetHashCode();
				return hashCode;
			}
		}
	}
}