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
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	[PublicAPI]
	public sealed class PsnInfoSystemNameChunk : PsnInfoPacketSubChunk, IEquatable<PsnInfoSystemNameChunk>
	{
		internal static PsnInfoSystemNameChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			return new PsnInfoSystemNameChunk(reader.ReadString(chunkHeader.DataLength));
		}

		public PsnInfoSystemNameChunk([NotNull] string systemName)
			: base(null)
		{
			if (systemName == null)
				throw new ArgumentNullException(nameof(systemName));

			SystemName = systemName;
		}

		public string SystemName { get; }

		public override ushort ChunkId => (ushort)PsnInfoPacketChunkId.PsnInfoSystemName;
		public override int DataLength => SystemName.Length;

		internal override void SerializeData(PsnBinaryWriter writer)
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
			return obj.GetType() == GetType() && Equals((PsnInfoSystemNameChunk)obj);
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