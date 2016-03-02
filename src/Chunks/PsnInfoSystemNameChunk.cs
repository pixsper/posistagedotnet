using System;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	internal class PsnInfoSystemNameChunk : PsnChunk
	{
		public static PsnInfoSystemNameChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			string systemName = reader.ReadString();

			return new PsnInfoSystemNameChunk(systemName);
		}

		public PsnInfoSystemNameChunk([NotNull] string systemName)
			: base(null)
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