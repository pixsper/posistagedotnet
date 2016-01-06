using System;
using System.IO;

namespace Imp.PosiStageDotNet.Serialization
{
	internal class PsnBinaryWriter : EndianBinaryWriter
	{
		public const int ChunkHeaderByteLength = 4;

		private static readonly EndianBitConverter BitConverterInstance = new LittleEndianBitConverter();

		public PsnBinaryWriter(Stream stream)
			: base(BitConverterInstance, stream)
		{
			
		}

		public void WriteChunkHeader(ushort id, int dataLength, bool hasSubChunks)
		{
			if (dataLength > short.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(dataLength), $"Chunk cannot contain more than {short.MaxValue} bytes");

			Write(id + (dataLength << 16) + (hasSubChunks ? 1 << 32 : 0));
		}
	}
}
