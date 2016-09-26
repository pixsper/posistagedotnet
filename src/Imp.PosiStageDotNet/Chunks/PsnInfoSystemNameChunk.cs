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
using System.Xml.Linq;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	/// <summary>
	///     PosiStageNet chunk containing the name of the system sending data
	/// </summary>
	[PublicAPI]
	public sealed class PsnInfoSystemNameChunk : PsnInfoPacketSubChunk, IEquatable<PsnInfoSystemNameChunk>
	{
		/// <exception cref="ArgumentNullException"><paramref name="systemName"/> is <see langword="null" />.</exception>
		public PsnInfoSystemNameChunk([NotNull] string systemName)
			: base(null)
		{
			if (systemName == null)
				throw new ArgumentNullException(nameof(systemName));

			SystemName = systemName;
		}

		/// <summary>
		///     Name of system sending PosiStageNet data
		/// </summary>
		public string SystemName { get; }

		/// <inheritdoc/>
		public override int DataLength => SystemName.Length;

		/// <inheritdoc/>
		public override PsnInfoPacketChunkId ChunkId => PsnInfoPacketChunkId.PsnInfoSystemName;

		/// <inheritdoc/>
		public bool Equals([CanBeNull] PsnInfoSystemNameChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && string.Equals(SystemName, other.SystemName);
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnInfoSystemNameChunk),
				new XAttribute(nameof(SystemName), SystemName));
		}

		/// <inheritdoc/>
		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnInfoSystemNameChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ SystemName.GetHashCode();
			}
		}

		internal static PsnInfoSystemNameChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			return new PsnInfoSystemNameChunk(reader.ReadString(chunkHeader.DataLength));
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(SystemName);
		}
	}
}