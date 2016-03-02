using System;
using System.Collections.Generic;
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

		protected static IEnumerable<Tuple<PsnChunkHeader, long>> FindSubChunkHeaders(PsnBinaryReader reader,
			int chunkDataLength)
		{
			var chunkHeaders = new List<Tuple<PsnChunkHeader, long>>();
			long startPos = reader.BaseStream.Position;

			while (true)
			{
				var chunkHeader = reader.ReadChunkHeader();
				chunkHeaders.Add(Tuple.Create(chunkHeader, reader.BaseStream.Position));

				if (reader.BaseStream.Position - startPos >= chunkDataLength)
					break;

				reader.Seek(chunkHeader.DataLength, SeekOrigin.Current);
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

		private void serializeChunks(PsnBinaryWriter writer, IEnumerable<PsnChunk> chunks)
		{
			foreach (var chunk in chunks)
			{
				writer.Write(ChunkHeader);
				chunk.SerializeData(writer);
				serializeChunks(writer, chunk.SubChunks);
			}
		}
	}



	internal class PsnUnknownChunk : PsnChunk
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
	}
}