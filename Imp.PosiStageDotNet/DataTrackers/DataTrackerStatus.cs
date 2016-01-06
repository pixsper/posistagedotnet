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

namespace Imp.PosiStageDotNet.DataTrackers
{
	public struct DataTrackerStatus : IDataTrackerId, IEquatable<DataTrackerStatus>
	{
		public DataTrackerStatus(float validity)
		{
			Validity = validity;
		}

		public PsnDataTrackerChunkId Id => PsnDataTrackerChunkId.PsnDataTrackerStatus;
		public int ByteLength => 4;

		public float Validity { get; }


		public bool Equals(DataTrackerStatus other)
		{
			return Validity.Equals(other.Validity);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			return obj is DataTrackerStatus && Equals((DataTrackerStatus)obj);
		}

		public override int GetHashCode()
		{
			return Validity.GetHashCode();
		}

		public static bool operator ==(DataTrackerStatus left, DataTrackerStatus right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DataTrackerStatus left, DataTrackerStatus right)
		{
			return !left.Equals(right);
		}
	}
}