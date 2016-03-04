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
using Imp.PosiStageDotNet.Chunks;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet
{
	/// <summary>
	///     Represents all possible data that can be contained in a PosiStageNet tracker
	/// </summary>
	[PublicAPI]
	public struct PsnTracker : IEquatable<PsnTracker>
	{
		public PsnTracker(int trackerId, string trackerName,
			Tuple<float, float, float> position = null,
			Tuple<float, float, float> speed = null,
			Tuple<float, float, float> orientation = null,
			Tuple<float, float, float> acceleration = null,
			Tuple<float, float, float> targetposition = null,
			float? validity = null)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in range {ushort.MinValue}-{ushort.MaxValue}");

			TrackerId = trackerId;

			if (trackerName == null)
				throw new ArgumentNullException(nameof(trackerName));

			TrackerName = trackerName;

			Position = position;
			Speed = speed;
			Orientation = orientation;
			Acceleration = acceleration;
			TargetPosition = targetposition;
			Validity = validity;
		}

		public int TrackerId { get; }
		public string TrackerName { get; }

		public Tuple<float, float, float> Position { get; }
		public Tuple<float, float, float> Speed { get; }
		public Tuple<float, float, float> Orientation { get; }
		public Tuple<float, float, float> Acceleration { get; }
		public Tuple<float, float, float> TargetPosition { get; }
		public float? Validity { get; }

		public IEnumerable<PsnDataTrackerSubChunk> ToChunks()
		{
			if (Position != null)
				yield return new PsnDataTrackerPosChunk(Position.Item1, Position.Item2, Position.Item3);

			if (Speed != null)
				yield return new PsnDataTrackerSpeedChunk(Speed.Item1, Speed.Item2, Speed.Item3);

			if (Orientation != null)
				yield return new PsnDataTrackerOriChunk(Orientation.Item1, Orientation.Item2, Orientation.Item3);

			if (Acceleration != null)
				yield return new PsnDataTrackerAccelChunk(Acceleration.Item1, Acceleration.Item2, Acceleration.Item3);

			if (TargetPosition != null)
				yield return new PsnDataTrackerTrgtPosChunk(TargetPosition.Item1, TargetPosition.Item2, TargetPosition.Item3);

			if (Validity.HasValue)
				yield return new PsnDataTrackerStatusChunk(Validity.Value);
		}

		public bool Equals(PsnTracker other)
		{
			return TrackerId == other.TrackerId && string.Equals(TrackerName, other.TrackerName)
			       && Equals(Position, other.Position) && Equals(Speed, other.Speed) && Equals(Orientation, other.Orientation)
			       && Equals(Acceleration, other.Acceleration) && Equals(TargetPosition, other.TargetPosition)
			       && Validity.Equals(other.Validity);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is PsnTracker && Equals((PsnTracker)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = TrackerId;
				hashCode = (hashCode * 397) ^ (TrackerName?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (Position?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (Speed?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (Orientation?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (Acceleration?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (TargetPosition?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ Validity.GetHashCode();
				return hashCode;
			}
		}
	}
}