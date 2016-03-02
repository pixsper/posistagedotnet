using System;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	internal class PsnInfoSystemNameChunk : PsnChunk, IEquatable<PsnInfoSystemNameChunk>
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
		public override int DataLength => SystemName.Length + 1;

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(SystemName);
		}

		public bool Equals([CanBeNull] PsnInfoSystemNameChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && string.Equals(SystemName, other.SystemName);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == this.GetType() && Equals((PsnInfoSystemNameChunk)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ SystemName.GetHashCode();
			}
		}
	}
}