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

namespace DBDesign.PosiStageDotNet.Chunks
{
	/// <summary>
	///     ID values for PosiStageNet chunks representing packet types
	/// </summary>
	[PublicAPI]
	public enum PsnPacketChunkId : ushort
	{
		/// <summary>
		///		Unknown PosiStageNet packet ID
		/// </summary>
		UnknownPacket = 0x0000,
		/// <summary>
		///		PosiStageNet data packet ID
		/// </summary>
		PsnDataPacket = 0x6755,
		/// <summary>
		///		PosiStageNet info packet ID
		/// </summary>
		PsnInfoPacket = 0x6756
	}


	/// <summary>
	///     ID values for PosiStageNet chunks present in an info packet
	/// </summary>
	[PublicAPI]
	public enum PsnInfoPacketChunkId : ushort
	{
		/// <summary>
		///		Info packet header chunk ID
		/// </summary>
		PsnInfoHeader = 0x0000,
		/// <summary>
		///		Info packet system name chunk ID
		/// </summary>
		PsnInfoSystemName = 0x0001,
		/// <summary>
		///		Info packet tracker list chunk ID
		/// </summary>
		PsnInfoTrackerList = 0x0002
	}


	/// <summary>
	///     ID values for PosiStageNet chunks present in an info tracker
	/// </summary>
	[PublicAPI]
	public enum PsnInfoTrackerChunkId : ushort
	{
		/// <summary>
		///		Info tracker name chunk ID
		/// </summary>
		PsnInfoTrackerName = 0x0000
	}


	/// <summary>
	///     ID values for PosiStageNet chunks present in an data packet
	/// </summary>
	[PublicAPI]
	public enum PsnDataPacketChunkId : ushort
	{
		/// <summary>
		///		Data packet header chunk ID
		/// </summary>
		PsnDataHeader = 0x0000,
		/// <summary>
		///		Data packet tracker chunk ID
		/// </summary>
		PsnDataTrackerList = 0x0001
	}


	/// <summary>
	///     Id values for PosiStageNet chunks present in an data tracker
	/// </summary>
	[PublicAPI]
	public enum PsnDataTrackerChunkId : ushort
	{
		/// <summary>
		///		Data tracker position chunk ID
		/// </summary>
		PsnDataTrackerPos = 0x0000,
		/// <summary>
		///		Data tracker speed chunk ID
		/// </summary>
		PsnDataTrackerSpeed = 0x0001,
		/// <summary>
		///		Data tracker orientation chunk ID
		/// </summary>
		PsnDataTrackerOri = 0x0002,
		/// <summary>
		///		Data tracker status chunk ID
		/// </summary>
		PsnDataTrackerStatus = 0x0003,
		/// <summary>
		///		Data tracker acceleration chunk ID
		/// </summary>
		PsnDataTrackerAccel = 0x0004,
		/// <summary>
		///		Data tracker target position chunk ID
		/// </summary>
		PsnDataTrackerTrgtPos = 0x0005,
        /// <summary>
        ///		Data tracker timestamp chunk ID
        /// </summary>
        PsnDataTrackerTimestamp = 0x0006
	}
}