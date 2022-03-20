// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using System.Text;
using Pixsper.PosiStageDotNet.Chunks;

namespace Pixsper.PosiStageDotNet.Serialization;

internal class PsnBinaryReader : EndianBinaryReader
{
	private static readonly EndianBitConverter BitConverterInstance = new LittleEndianBitConverter();

	public PsnBinaryReader(Stream stream)
		: base(BitConverterInstance, stream, Encoding.UTF8) { }

	public PsnChunkHeader ReadChunkHeader()
	{
		return PsnChunkHeader.FromUInt32(ReadUInt32());
	}

	public string ReadString(int length)
	{
		return Encoding.GetString(ReadBytes(length), 0, length);
	}
}