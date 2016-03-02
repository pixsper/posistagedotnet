using System;
using System.Collections.Generic;
using Imp.PosiStageDotNet.Serialization;

namespace Imp.PosiStageDotNet.Chunks
{
	internal class PsnInfoSystemNameChunk : PsnChunk
	{
		public PsnInfoSystemNameChunk([NotNull] string systemName, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
		{
			if (systemName == null)
				throw new ArgumentNullException(nameof(systemName));

			SystemName = systemName;
		}

		public string SystemName { get; }

		public override ushort ChunkId => (ushort)PsnInfoChunkId.PsnInfoSystemName;
		public override int DataLength => SystemName.Length;

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(SystemName);
		}
	}
}
