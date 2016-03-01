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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet
{
	[PublicAPI]
	public class PsnInfoPacket : PsnPacket
	{
		const int HeaderByteLength = 12;

		public PsnInfoPacket(ulong timestamp, int versionHigh, int versionLow, int frameId, int framePacketCount,
			string systemName,
			IDictionary<ushort, string> trackerNames)
		{
			TimeStamp = timestamp;

			if (versionHigh < 0 || versionHigh > 255)
				throw new ArgumentOutOfRangeException(nameof(versionHigh), "versionHigh must be between 0 and 255");

			VersionHigh = versionHigh;

			if (versionLow < 0 || versionLow > 255)
				throw new ArgumentOutOfRangeException(nameof(versionLow), "versionLow must be between 0 and 255");

			VersionLow = versionLow;

			if (frameId < 0 || frameId > 255)
				throw new ArgumentOutOfRangeException(nameof(frameId), "frameId must be between 0 and 255");

			FrameId = frameId;

			if (framePacketCount < 0 || framePacketCount > 255)
				throw new ArgumentOutOfRangeException(nameof(framePacketCount), "framePacketCount must be between 0 and 255");

			FramePacketCount = framePacketCount;

			if (string.IsNullOrEmpty(systemName))
				throw new ArgumentException("System name cannot be null or empty", nameof(systemName));

			SystemName = systemName;

			if (trackerNames == null)
				throw new ArgumentNullException(nameof(trackerNames));

			TrackerNames = new Dictionary<ushort, string>(trackerNames);
		}

		public ulong TimeStamp { get; }

		public int VersionHigh { get; }
		public int VersionLow { get; }
		public int FrameId { get; }
		public int FramePacketCount { get; }

		public string SystemName { get; }

		public IReadOnlyDictionary<ushort, string> TrackerNames { get; }

		public override PsnPacketChunkId Id => PsnPacketChunkId.PsnInfoPacket;



		public override byte[] ToByteArray()
		{
			int trackerListChunkByteLength = TrackerNames.Sum(p => PsnBinaryWriter.ChunkHeaderByteLength * 2 + p.Value.Length);

			int rootChunkByteLength = PsnBinaryWriter.ChunkHeaderByteLength + HeaderByteLength + trackerListChunkByteLength;

			using (var ms = new MemoryStream())
			using (var writer = new PsnBinaryWriter(ms))
			{
				writer.WriteChunkHeader((ushort)Id, rootChunkByteLength, true);

				// Write header
				writer.WriteChunkHeader((ushort)PsnInfoChunkId.PsnInfoPacketHeader, HeaderByteLength, false);
				writer.Write(TimeStamp);
				writer.Write(VersionHigh);
				writer.Write(VersionLow);
				writer.Write(FrameId);
				writer.Write(FramePacketCount);

				// Write tracker List
				writer.WriteChunkHeader((ushort)PsnInfoChunkId.PsnInfoTrackerList, trackerListChunkByteLength, true);

				foreach (var pair in TrackerNames)
				{
					writer.WriteChunkHeader(pair.Key, PsnBinaryWriter.ChunkHeaderByteLength + pair.Value.Length, true);
					writer.WriteChunkHeader((ushort)PsnInfoTrackerChunkId.PsnInfoTrackerName, pair.Value.Length, false);
					writer.Write(pair.Value);
				}

				return ms.ToArray();
			}
		}

		internal static PsnInfoPacket Deserialize(PsnBinaryReader reader)
		{
			throw new NotImplementedException();
		}
	}
}