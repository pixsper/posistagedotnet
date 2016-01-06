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
using Imp.PosiStageDotNet.Serialization;

namespace Imp.PosiStageDotNet.DataTrackers
{
	public class PsnTrackerStatus : PsnTrackerElement, IEquatable<PsnTrackerStatus>
	{
		public PsnTrackerStatus(float validity)
		{
			Validity = validity;
		}

		public override PsnDataTrackerChunkId Id => PsnDataTrackerChunkId.PsnDataTrackerStatus;
		public override int ByteLength => 4;

		public float Validity { get; }

		public bool Equals(PsnTrackerStatus other)
		{
			return Validity.Equals(other.Validity);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			return obj is PsnTrackerStatus && Equals((PsnTrackerStatus)obj);
		}

		public override int GetHashCode()
		{
			return Validity.GetHashCode();
		}

		public static bool operator ==(PsnTrackerStatus left, PsnTrackerStatus right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PsnTrackerStatus left, PsnTrackerStatus right)
		{
			return !left.Equals(right);
		}

		internal override void Serialize(PsnBinaryWriter writer)
		{
			writer.WriteChunkHeader((ushort)Id, ByteLength, false);
			writer.Write(Validity);
		}
	}
}