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
using System.Text;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Serialization
{
	internal class PsnBinaryWriter : EndianBinaryWriter
	{
		public const int ChunkHeaderByteLength = 4;

		private static readonly EndianBitConverter BitConverterInstance = new LittleEndianBitConverter();

		public PsnBinaryWriter(Stream stream)
			: base(BitConverterInstance, stream, Encoding.UTF8) { }

		public void Write(PsnChunkHeader chunkHeader)
		{
			Write(chunkHeader.ToUInt32());
		}

		public override void Write([CanBeNull] string value)
		{
			base.Write(Encoding.GetBytes((value ?? string.Empty) + "\0"));
		}
	}
}