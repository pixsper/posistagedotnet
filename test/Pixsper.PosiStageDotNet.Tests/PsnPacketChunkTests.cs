﻿// This file is part of PosiStageDotNet.
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixsper.PosiStageDotNet.Chunks;

namespace Pixsper.PosiStageDotNet.Tests;

[TestClass]
public class PsnPacketChunkTests
{
	[TestMethod]
	public void CanSerializeAndDeserialize()
	{
		var infoPacket1 =
			new PsnInfoPacketChunk(
				new PsnInfoHeaderChunk(1500, 2, 1, 34, 1),
				new PsnInfoSystemNameChunk("Test System"),
				new PsnInfoTrackerListChunk(
					new PsnInfoTrackerChunk(0,
						new PsnInfoTrackerNameChunk("Test Tracker"))
				)
			);

		var infoData = infoPacket1.ToByteArray();

		var infoPacket2 = PsnPacketChunk.FromByteArray(infoData);

		infoPacket1.Should()
			.Be(infoPacket2, "because the deserializing the serialized data should produce the same values");

		var dataPacket1 =
			new PsnDataPacketChunk(
				new PsnDataHeaderChunk(1500, 2, 1, 34, 1),
				new PsnDataTrackerListChunk(
					new PsnDataTrackerChunk(0,
						new PsnDataTrackerPosChunk(0.45f, 7.56f, 2343.43f),
						new PsnDataTrackerSpeedChunk(0.34f, 5.76f, -876.87f),
						new PsnDataTrackerOriChunk(6.4f, -3.576f, 3873.3f),
						new PsnDataTrackerStatusChunk(54f),
						new PsnDataTrackerAccelChunk(4.34f, 23423.5f, 234.4f),
						new PsnDataTrackerTrgtPosChunk(23.3f, 4325f, 4234f),
						new PsnDataTrackerTimestampChunk(34524534454543543)
					),
					new PsnDataTrackerChunk(1,
						new PsnDataTrackerPosChunk(-343.44f, 4.76f, 2.45f),
						new PsnDataTrackerSpeedChunk(34f, -23f, 5676.4f),
						new PsnDataTrackerOriChunk(24.7f, 3.53376f, 38.334f),
						new PsnDataTrackerStatusChunk(0.1f),
						new PsnDataTrackerAccelChunk(4234.34f, 543543.4f, 23.43f),
						new PsnDataTrackerTrgtPosChunk(2342.6f, 35.5f, -14545.4f),
						new PsnDataTrackerTimestampChunk(ulong.MaxValue)
					)
				)
			);

		var dataData = dataPacket1.ToByteArray();

		var dataPacket2 = PsnPacketChunk.FromByteArray(dataData);

		dataPacket1.Should()
			.Be(dataPacket2, "because the deserializing the serialized data should produce the same values");
	}
}