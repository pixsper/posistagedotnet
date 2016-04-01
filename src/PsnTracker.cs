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
	///     Immutable class representing the entire state of a single PosiStageNet tracker
	/// </summary>
	[PublicAPI]
	public struct PsnTracker : IEquatable<PsnTracker>
	{
		/// <summary>
		///     Creates a copy of a PsnTracker with updated values
		/// </summary>
		/// <param name="sourceTracker">Tracker to use as clone source</param>
		/// <param name="trackerName">New tracker name value, or null to use source value</param>
		/// <param name="clearPosition">If true, ignores new position value and clears value in clone</param>
		/// <param name="position">New position value, or null to use source value</param>
		/// <param name="clearSpeed">If true, ignores new speed value and clears value in clone</param>
		/// <param name="speed">New speed value, or null to use source value</param>
		/// <param name="clearOrientation">If true, ignores new orientation value and clears value in clone</param>
		/// <param name="orientation">New orientation value, or null to use source value</param>
		/// <param name="clearAcceleration">If true, ignores new acceleration value and clears value in clone</param>
		/// <param name="acceleration">New acceleration value, or null to use source value</param>
		/// <param name="clearTargetPosition">If true, ignores new target position value and clears value in clone</param>
		/// <param name="targetposition">New targetposition value, or null to use source value</param>
		/// <param name="clearValidity">If true, ignores new validity value and clears value in clone</param>
		/// <param name="validity">New validity value, or null to use source value</param>
		public static PsnTracker Clone(PsnTracker sourceTracker,
			string trackerName = null,
			bool clearPosition = false, Tuple<float, float, float> position = null,
			bool clearSpeed = false, Tuple<float, float, float> speed = null,
			bool clearOrientation = false, Tuple<float, float, float> orientation = null,
			bool clearAcceleration = false, Tuple<float, float, float> acceleration = null,
			bool clearTargetPosition = false, Tuple<float, float, float> targetposition = null,
			bool clearValidity = false, float? validity = null)
		{
			return new PsnTracker(sourceTracker.TrackerId,
				trackerName ?? sourceTracker.TrackerName,
				clearPosition ? null : position ?? sourceTracker.Position,
				clearSpeed ? null : speed ?? sourceTracker.Speed,
				clearOrientation ? null : orientation ?? sourceTracker.Orientation,
				clearAcceleration ? null : acceleration ?? sourceTracker.Acceleration,
				clearTargetPosition ? null : targetposition ?? sourceTracker.TargetPosition,
				clearValidity ? null : validity ?? sourceTracker.Validity);
		}

		/// <summary>
		///     Clone constructor provided to set properties which consumers of the library shouldn't be able to set (only relevant
		///     for
		///     packets received from a remote server)
		/// </summary>
		internal static PsnTracker CloneInternal(PsnTracker sourceTracker,
			bool clearTrackerName = false, string trackerName = null,
			bool clearDataTimeStamp = false, ulong? dataTimeStamp = null,
			bool clearInfoTimeStamp = false, ulong? infoTimeStamp = null,
			bool clearPosition = false, Tuple<float, float, float> position = null,
			bool clearSpeed = false, Tuple<float, float, float> speed = null,
			bool clearOrientation = false, Tuple<float, float, float> orientation = null,
			bool clearAcceleration = false, Tuple<float, float, float> acceleration = null,
			bool clearTargetPosition = false, Tuple<float, float, float> targetposition = null,
			bool clearValidity = false, float? validity = null)
		{
			return new PsnTracker(sourceTracker.TrackerId,
				clearTrackerName ? null : trackerName ?? sourceTracker.TrackerName,
				clearDataTimeStamp ? null : dataTimeStamp ?? sourceTracker.DataTimeStamp,
				clearInfoTimeStamp ? null : infoTimeStamp ?? sourceTracker.InfoTimeStamp,
				clearPosition ? null : position ?? sourceTracker.Position,
				clearSpeed ? null : speed ?? sourceTracker.Speed,
				clearOrientation ? null : orientation ?? sourceTracker.Orientation,
				clearAcceleration ? null : acceleration ?? sourceTracker.Acceleration,
				clearTargetPosition ? null : targetposition ?? sourceTracker.TargetPosition,
				clearValidity ? null : validity ?? sourceTracker.Validity);
		}

		public PsnTracker(int trackerId, string trackerName,
			Tuple<float, float, float> position = null,
			Tuple<float, float, float> speed = null,
			Tuple<float, float, float> orientation = null,
			Tuple<float, float, float> acceleration = null,
			Tuple<float, float, float> targetPosition = null,
			float? validity = null)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in range {ushort.MinValue}-{ushort.MaxValue}");

			TrackerId = trackerId;

			if (trackerName == null)
				throw new ArgumentNullException(nameof(trackerName));

			TrackerName = trackerName;

			DataTimeStamp = null;
			InfoTimeStamp = null;

			Position = position;
			Speed = speed;
			Orientation = orientation;
			Acceleration = acceleration;
			TargetPosition = targetPosition;
			Validity = validity;
		}

		/// <summary>
		///     Constructor provided to set properties which consumers of the library shouldn't be able to set (only relevant for
		///     packets received from a remote server)
		/// </summary>
		internal PsnTracker(int trackerId, string trackerName = null, ulong? dataTimeStamp = null, ulong? infoTimeStamp = null,
			Tuple<float, float, float> position = null,
			Tuple<float, float, float> speed = null,
			Tuple<float, float, float> orientation = null,
			Tuple<float, float, float> acceleration = null,
			Tuple<float, float, float> targetPosition = null,
			float? validity = null)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in range {ushort.MinValue}-{ushort.MaxValue}");

			TrackerId = trackerId;
			DataTimeStamp = dataTimeStamp;
			InfoTimeStamp = infoTimeStamp;
			TrackerName = trackerName;

			Position = position;
			Speed = speed;
			Orientation = orientation;
			Acceleration = acceleration;
			TargetPosition = targetPosition;
			Validity = validity;
		}

		/// <summary>
		///     Unique ID of tracker
		/// </summary>
		public int TrackerId { get; }

		/// <summary>
		///     Name of tracker, or null if not yet received from remote server
		/// </summary>
		[CanBeNull]
		public string TrackerName { get; }

		/// <summary>
		///     Time stamp of last time data received from remote server, or null if tracker has not been received from remote
		///     server
		/// </summary>
		public ulong? DataTimeStamp { get; }

		/// <summary>
		///     Time stamp of last time info received from remote server, or null if tracker has not been received from remote
		///     server
		/// </summary>
		public ulong? InfoTimeStamp { get; }



		public Tuple<float, float, float> Position { get; }
		public Tuple<float, float, float> Speed { get; }
		public Tuple<float, float, float> Orientation { get; }
		public Tuple<float, float, float> Acceleration { get; }
		public Tuple<float, float, float> TargetPosition { get; }
		public float? Validity { get; }



		/// <summary>
		///     Creates a copy of this tracker with an updated tracker name value, or clears value if null
		/// </summary>
		[Pure]
		internal PsnTracker SetTrackerNameInternal([CanBeNull] string trackerName)
			=> CloneInternal(this, clearTrackerName: trackerName == null, trackerName: trackerName);

		/// <summary>
		///     Creates a copy of this tracker with an updated data time stamp value
		/// </summary>
		[Pure]
		internal PsnTracker SetDataTimeStamp([CanBeNull] ulong? dataTimeStamp)
			=> CloneInternal(this, clearDataTimeStamp: dataTimeStamp == null, dataTimeStamp: dataTimeStamp);

		/// <summary>
		///     Creates a copy of this tracker with an updated info time stamp value, or clears value if null
		/// </summary>
		[Pure]
		internal PsnTracker SetInfoTimeStamp([CanBeNull] ulong? infoTimeStamp)
			=> CloneInternal(this, clearInfoTimeStamp: infoTimeStamp == null, infoTimeStamp: infoTimeStamp);



		/// <summary>
		///     Creates a copy of this tracker with an updated tracker name value
		/// </summary>
		[Pure]
		public PsnTracker SetTrackerName([NotNull] string trackerName) => Clone(this, trackerName: trackerName);

		/// <summary>
		///     Creates a copy of this tracker with an updated position value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker SetPosition([CanBeNull] Tuple<float, float, float> position)
			=> Clone(this, clearPosition: position == null, position: position);

		/// <summary>
		///     Creates a copy of this tracker with an updated speed value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker SetSpeed([CanBeNull] Tuple<float, float, float> speed)
			=> Clone(this, clearSpeed: speed == null, speed: speed);

		/// <summary>
		///     Creates a copy of this tracker with an updated orientation value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker SetOrientation([CanBeNull] Tuple<float, float, float> orientation)
			=> Clone(this, clearOrientation: orientation == null, orientation: orientation);

		/// <summary>
		///     Creates a copy of this tracker with an updated acceleration value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker SetAcceleration([CanBeNull] Tuple<float, float, float> acceleration)
			=> Clone(this, clearAcceleration: acceleration == null, acceleration: acceleration);

		/// <summary>
		///     Creates a copy of this tracker with an updated target position value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker SetTargetPosition([CanBeNull] Tuple<float, float, float> targetPosition)
			=> Clone(this, clearTargetPosition: targetPosition == null, targetposition: targetPosition);

		/// <summary>
		///     Creates a copy of this tracker with an updated validity value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker SetValidity([CanBeNull] float? validity)
			=> Clone(this, clearValidity: validity == null, validity: validity);


		/// <summary>
		///     Creates an enumerable of PsnDataTrackerChunks representing the data contained in this tracker
		/// </summary>
		public IEnumerable<PsnDataTrackerSubChunk> ToDataTrackerChunks()
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

		/// <summary>
		///     Creates an enumerable of PsnInfoTrackerChunks representing the info contained in this tracker
		/// </summary>
		public IEnumerable<PsnInfoTrackerSubChunk> ToInfoTrackerChunks()
		{
			if (TrackerName != null)
				yield return new PsnInfoTrackerNameChunk(TrackerName);
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

		public override string ToString()
		{
			return
				$"PsnTracker: Id {TrackerId}" +
				", Name " + (TrackerName ?? "(Unknown)") +
				", InfoTimeStamp " + (InfoTimeStamp.HasValue ? TimeSpan.FromMilliseconds(InfoTimeStamp.Value).ToString() : "None") +
				", DataTimeStamp " + (DataTimeStamp.HasValue ? TimeSpan.FromMilliseconds(DataTimeStamp.Value).ToString() : "None") +
				(Position != null ? $", Position {Position}" : string.Empty) +
				(Speed != null ? $", Speed {Speed}" : string.Empty) +
				(Orientation != null ? $", Orientation {Orientation}" : string.Empty) +
				(Acceleration != null ? $", Acceleration {Acceleration}" : string.Empty) +
				(TargetPosition != null ? $", Target Position {TargetPosition}" : string.Empty) +
				(Validity != null ? $", Validity {Validity}" : string.Empty);
		}
	}
}