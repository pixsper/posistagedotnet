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

using System.IO;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet
{
	[PublicAPI]
	public abstract class PsnPacket
	{
		[CanBeNull]
		public static PsnPacket FromByteArray(byte[] data)
		{
			using (var ms = new MemoryStream(data))
			using (var reader = new PsnBinaryReader(ms))
			{
				var packetChunkHeader = reader.ReadChunkHeader();

				if (data.Length != PsnBinaryReader.ChunkHeaderByteLength + packetChunkHeader.Item2)
				{
					// Length does not match the reported length of the packet
					return null;
				}

				if (!packetChunkHeader.Item3)
				{
					// Packet reports root chunk contains no children
					return null;
				}

				switch ((PsnPacketChunkId)packetChunkHeader.Item1)
				{
					case PsnPacketChunkId.PsnDataPacket:
						return PsnDataPacket.Deserialize(reader);
						
					case PsnPacketChunkId.PsnInfoPacket:
						return PsnInfoPacket.Deserialize(reader);

					default:
						// Unknown packet type
						return null;
				}
			}
		}

		public abstract PsnPacketChunkId Id { get; }

		public abstract byte[] ToByteArray();
	}
}