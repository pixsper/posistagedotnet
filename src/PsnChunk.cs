using System.Collections.Generic;
using System.IO;
using System.Linq;
using Imp.PosiStageDotNet.Serialization;

namespace Imp.PosiStageDotNet
{
	abstract class PsnChunk
	{
		public const int ChunkHeaderLength = 4;

		protected PsnChunk([CanBeNull] IEnumerable<PsnChunk> subChunks)
		{
			SubChunks = subChunks ?? Enumerable.Empty<PsnChunk>();
		}

		/// <summary>
		/// The 16-bit identifier for this chunk
		/// </summary>
		public abstract ushort ChunkId { get; }

		/// <summary>
		/// The length of the data contained within this chunk, excluding sub-chunks and the local chunk header.
		/// </summary>
		public abstract int DataLength { get; }

		/// <summary>
		/// The length of the entire chunk, including sub-chunks but excluding the local chunk header.
		/// </summary>
		public int ChunkLength => DataLength + SubChunks.Sum(c => ChunkHeaderLength + c.ChunkLength);

		/// <summary>
		/// Enumerable of sub-chunks
		/// </summary>
		public IEnumerable<PsnChunk> SubChunks { get; }

		/// <summary>
		/// True if this chunk contains sub-chunks
		/// </summary>
		public bool HasSubChunks => SubChunks.Any();

		/// <summary>
		/// Serializes the chunk to a byte array
		/// </summary>
		public byte[] ToByteArray()
		{
			using (var ms = new MemoryStream(ChunkHeaderLength + ChunkLength))
			using (var writer = new PsnBinaryWriter(ms))
			{
				writer.WriteChunkHeader(ChunkId, DataLength, HasSubChunks);
				SerializeData(writer);
				serializeChunks(writer, SubChunks);

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Serializes any data contained within this chunk, or nothing if the chunk contains no data
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void SerializeData(PsnBinaryWriter writer) { }

		private void serializeChunks(PsnBinaryWriter writer, IEnumerable<PsnChunk> chunks)
		{
			foreach (var chunk in chunks)
			{
				writer.WriteChunkHeader(chunk.ChunkId, chunk.DataLength, chunk.HasSubChunks);
				chunk.SerializeData(writer);
				serializeChunks(writer, chunk.SubChunks);
			}
		}
	}
}
