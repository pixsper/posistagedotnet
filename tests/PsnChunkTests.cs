using FluentAssertions;
using Imp.PosiStageDotNet.Chunks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imp.PosiStageDotNet.Tests
{
	[TestClass]
	public class PsnChunkTests
	{
		[TestMethod]
		public void CanSerializeAndDeserialize()
		{
			var infoPacket1 = 
				new PsnInfoPacketChunk(
					new PsnInfoPacketHeaderChunk(1500, 2, 1, 34, 1),
					new PsnInfoSystemNameChunk("Test System"),
					new PsnInfoTrackerListChunk(
						new PsnInfoTrackerChunk(0,
							new PsnInfoTrackerName("Test Tracker"))
						)
				);

			var infoData = infoPacket1.ToByteArray();

			var infoPacket2 = PsnChunk.FromByteArray(infoData);

			infoPacket1.Should().Be(infoPacket2, "because the deserializing the serialized data should produce the same values");

			var dataPacket1 =
				new PsnDataPacketChunk(
					new PsnDataPacketHeaderChunk(1500, 2, 1, 34, 1),
					new PsnDataTrackerListChunk(
						new PsnDataTrackerChunk(0,
							new PsnDataTrackerPosChunk(0.45f, 7.56f, 2343.43f),
							new PsnDataTrackerSpeedChunk(0.34f, 5.76f, -876.87f),
							new PsnDataTrackerOriChunk(6.4f, -3.576f, 3873.3f),
							new PsnDataTrackerStatusChunk(54f),
							new PsnDataTrackerAccelChunk(4.34f, 23423.5f, 234.4f),
							new PsnDataTrackerTrgtPosChunk(23.3f, 4325f, 4234f)
						),
						new PsnDataTrackerChunk(1,
							new PsnDataTrackerPosChunk(-343.44f, 4.76f, 2.45f),
							new PsnDataTrackerSpeedChunk(34f, -23f, 5676.4f),
							new PsnDataTrackerOriChunk(24.7f, 3.53376f, 38.334f),
							new PsnDataTrackerStatusChunk(0.1f),
							new PsnDataTrackerAccelChunk(4234.34f, 543543.4f, 23.43f),
							new PsnDataTrackerTrgtPosChunk(2342.6f, 35.5f, -14545.4f)
						)
					)
				);

			var dataData = dataPacket1.ToByteArray();

			var dataPacket2 = PsnChunk.FromByteArray(dataData);

			dataPacket1.Should().Be(dataPacket2, "because the deserializing the serialized data should produce the same values");
		}
	}
}
