// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using System.Text;
using Pixsper.PosiStageDotNet.Chunks;

namespace Pixsper.PosiStageDotNet.Serialization;

internal class PsnBinaryWriter : EndianBinaryWriter
{
	private static readonly EndianBitConverter BitConverterInstance = new LittleEndianBitConverter();

	public PsnBinaryWriter(Stream stream)
		: base(BitConverterInstance, stream, Encoding.UTF8) { }

	public void Write(PsnChunkHeader chunkHeader)
	{
		Write(chunkHeader.ToUInt32());
	}

	public override void Write(string? value)
	{
		base.Write(Encoding.GetBytes(value ?? string.Empty));
	}
}