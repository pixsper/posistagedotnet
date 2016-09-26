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

using JetBrains.Annotations;
#pragma warning disable 1591

namespace Imp.PosiStageDotNet.Chunks
{
	/// <summary>
	///     Id values for PosiStageNet chunks representing packet types
	/// </summary>
	[PublicAPI]
	public enum PsnPacketChunkId : ushort
	{
		UnknownPacket = 0x0000,
		PsnDataPacket = 0x6755,
		PsnInfoPacket = 0x6756
	}


	/// <summary>
	///     Id values for PosiStageNet chunks present in an info packet
	/// </summary>
	[PublicAPI]
	public enum PsnInfoPacketChunkId : ushort
	{
		PsnInfoHeader = 0x0000,
		PsnInfoSystemName = 0x0001,
		PsnInfoTrackerList = 0x0002
	}


	/// <summary>
	///     Id values for PosiStageNet chunks present in an info tracker
	/// </summary>
	[PublicAPI]
	public enum PsnInfoTrackerChunkId : ushort
	{
		PsnInfoTrackerName = 0x0000
	}


	/// <summary>
	///     Id values for PosiStageNet chunks present in an data packet
	/// </summary>
	[PublicAPI]
	public enum PsnDataPacketChunkId : ushort
	{
		PsnDataHeader = 0x0000,
		PsnDataTrackerList = 0x0001
	}


	/// <summary>
	///     Id values for PosiStageNet chunks present in an data tracker
	/// </summary>
	[PublicAPI]
	public enum PsnDataTrackerChunkId : ushort
	{
		PsnDataTrackerPos = 0x0000,
		PsnDataTrackerSpeed = 0x0001,
		PsnDataTrackerOri = 0x0002,
		PsnDataTrackerStatus = 0x0003,
		PsnDataTrackerAccel = 0x0004,
		PsnDataTrackerTrgtPos = 0x0005
	}
}