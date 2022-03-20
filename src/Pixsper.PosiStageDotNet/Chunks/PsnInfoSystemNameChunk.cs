// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Xml.Linq;
using Pixsper.PosiStageDotNet.Serialization;

namespace Pixsper.PosiStageDotNet.Chunks;

/// <summary>
///     PosiStageNet chunk containing the name of the system sending data
/// </summary>
public sealed class PsnInfoSystemNameChunk : PsnInfoPacketSubChunk, IEquatable<PsnInfoSystemNameChunk>
{
	/// <exception cref="ArgumentNullException"><paramref name="systemName"/> is <see langword="null" />.</exception>
	public PsnInfoSystemNameChunk(string systemName)
		: base(null)
	{
		SystemName = systemName ?? throw new ArgumentNullException(nameof(systemName));
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
	public bool Equals(PsnInfoSystemNameChunk? other)
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
	public override bool Equals(object? obj)
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