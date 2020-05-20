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
using DBDesign.PosiStageDotNet.Chunks;
using JetBrains.Annotations;

namespace DBDesign.PosiStageDotNet
{
	/// <summary>
	///     Immutable class representing the entire state of a single PosiStageNet tracker
	/// </summary>
	[PublicAPI]
	public readonly struct PsnTracker : IEquatable<PsnTracker>
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
        /// <param name="targetPosition">New target position value, or null to use source value</param>
        /// <param name="clearTimestamp">If true, ignores new timestamp value and clears value in clone</param>
        /// <param name="timestamp">New timestamp value, or null to use source value</param>
        /// <param name="clearValidity">If true, ignores new validity value and clears value in clone</param>
        /// <param name="validity">New validity value, or null to use source value</param>
        public static PsnTracker Clone(PsnTracker sourceTracker,
			string trackerName = null,
			bool clearPosition = false, Tuple<float, float, float> position = null,
			bool clearSpeed = false, Tuple<float, float, float> speed = null,
			bool clearOrientation = false, Tuple<float, float, float> orientation = null,
			bool clearAcceleration = false, Tuple<float, float, float> acceleration = null,
			bool clearTargetPosition = false, Tuple<float, float, float> targetPosition = null,
            bool clearTimestamp = false, ulong? timestamp = null,
			bool clearValidity = false, float? validity = null)
        {
            return new PsnTracker(sourceTracker.TrackerId,
                trackerName ?? sourceTracker.TrackerName,
                clearPosition ? null : position ?? sourceTracker.Position,
                clearSpeed ? null : speed ?? sourceTracker.Speed,
                clearOrientation ? null : orientation ?? sourceTracker.Orientation,
                clearAcceleration ? null : acceleration ?? sourceTracker.Acceleration,
                clearTargetPosition ? null : targetPosition ?? sourceTracker.TargetPosition,
                clearTimestamp ? null : timestamp ?? sourceTracker.Timestamp,
				clearValidity ? null : validity ?? sourceTracker.Validity);
		}

		/// <summary>
		///     Clone constructor provided to set properties which consumers of the library shouldn't be able to set (only relevant
		///     for packets received from a remote server)
		/// </summary>
		internal static PsnTracker CloneInternal(PsnTracker sourceTracker,
			bool clearTrackerName = false, string trackerName = null,
			bool clearDataLastReceived = false, ulong? dataLastReceived = null,
			bool clearInfoLastReceived = false, ulong? infoLastReceived = null,
			bool clearPosition = false, Tuple<float, float, float> position = null,
			bool clearSpeed = false, Tuple<float, float, float> speed = null,
			bool clearOrientation = false, Tuple<float, float, float> orientation = null,
			bool clearAcceleration = false, Tuple<float, float, float> acceleration = null,
			bool clearTargetPosition = false, Tuple<float, float, float> targetPosition = null,
			bool clearTimestamp = false, ulong? timestamp = null,
			bool clearValidity = false, float? validity = null)
		{
			return new PsnTracker(sourceTracker.TrackerId,
				clearTrackerName ? null : trackerName ?? sourceTracker.TrackerName,
				clearDataLastReceived ? null : dataLastReceived ?? sourceTracker.DataLastReceived,
				clearInfoLastReceived ? null : infoLastReceived ?? sourceTracker.InfoLastReceived,
				clearPosition ? null : position ?? sourceTracker.Position,
				clearSpeed ? null : speed ?? sourceTracker.Speed,
				clearOrientation ? null : orientation ?? sourceTracker.Orientation,
				clearAcceleration ? null : acceleration ?? sourceTracker.Acceleration,
				clearTargetPosition ? null : targetPosition ?? sourceTracker.TargetPosition,
				clearTimestamp ? null : timestamp ?? sourceTracker.Timestamp,
				clearValidity ? null : validity ?? sourceTracker.Validity);
		}

		/// <exception cref="ArgumentNullException"><paramref name="trackerName"/> is <see langword="null" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public PsnTracker(int trackerId, [CanBeNull] string trackerName,
			Tuple<float, float, float> position = null,
			Tuple<float, float, float> speed = null,
			Tuple<float, float, float> orientation = null,
			Tuple<float, float, float> acceleration = null,
			Tuple<float, float, float> targetPosition = null,
			ulong? timestamp = null,
			float? validity = null)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in range {ushort.MinValue}-{ushort.MaxValue}");

			TrackerId = trackerId;

			TrackerName = trackerName;

			DataLastReceived = null;
			InfoLastReceived = null;

			Position = position;
			Speed = speed;
			Orientation = orientation;
			Acceleration = acceleration;
			TargetPosition = targetPosition;
            Timestamp = timestamp;
			Validity = validity;
		}

		/// <summary>
		///     Constructor provided to set properties which consumers of the library shouldn't be able to set (only relevant for
		///     packets received from a remote server)
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		internal PsnTracker(int trackerId, string trackerName = null, ulong? dataLastReceived = null,
			ulong? infoLastReceived = null,
			Tuple<float, float, float> position = null,
			Tuple<float, float, float> speed = null,
			Tuple<float, float, float> orientation = null,
			Tuple<float, float, float> acceleration = null,
			Tuple<float, float, float> targetPosition = null,
			ulong? timestamp = null,
			float? validity = null)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in range {ushort.MinValue}-{ushort.MaxValue}");

			TrackerId = trackerId;
			DataLastReceived = dataLastReceived;
			InfoLastReceived = infoLastReceived;
			TrackerName = trackerName;

			Position = position;
			Speed = speed;
			Orientation = orientation;
			Acceleration = acceleration;
			TargetPosition = targetPosition;
            Timestamp = timestamp;
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
		///     Timestamp of last time data received from remote server, or null if tracker has not been received from remote
		///     server
		/// </summary>
		public ulong? DataLastReceived { get; }

		/// <summary>
		///     Timestamp of last time info received from remote server, or null if tracker has not been received from remote
		///     server
		/// </summary>
		public ulong? InfoLastReceived { get; }


		/// <summary>
		///		Tracker position vector in m
		/// </summary>
		public Tuple<float, float, float> Position { get; }

		/// <summary>
		///		Tracker speed vector in m/s
		/// </summary>
		public Tuple<float, float, float> Speed { get; }

		/// <summary>
		///		Tracker absolute orientation around rotation axis in radians
		/// </summary>
		public Tuple<float, float, float> Orientation { get; }

		/// <summary>
		///		Tracker acceleration vector in m/s^2
		/// </summary>
		public Tuple<float, float, float> Acceleration { get; }

		/// <summary>
		///		Tracker target position vector in m
		/// </summary>
		public Tuple<float, float, float> TargetPosition { get; }

		/// <summary>
		///		Time in microseconds at which the data in this tracker was measured
		/// </summary>
		public ulong? Timestamp { get; }

		/// <summary>
		///		Tracker validity
		/// </summary>
		public float? Validity { get; }



		/// <summary>
		///     Creates a copy of this tracker with an updated tracker name value, or clears value if null
		/// </summary>
		[Pure]
		internal PsnTracker WithTrackerNameInternal([CanBeNull] string trackerName)
			=> CloneInternal(this, clearTrackerName: trackerName == null, trackerName: trackerName);

		/// <summary>
		///     Creates a copy of this tracker with an updated data time stamp value
		/// </summary>
		[Pure]
		internal PsnTracker WithDataTimeStamp([CanBeNull] ulong? dataTimeStamp)
			=> CloneInternal(this, clearDataLastReceived: dataTimeStamp == null, dataLastReceived: dataTimeStamp);

		/// <summary>
		///     Creates a copy of this tracker with an updated info time stamp value, or clears value if null
		/// </summary>
		[Pure]
		internal PsnTracker WithInfoTimeStamp([CanBeNull] ulong? infoTimeStamp)
			=> CloneInternal(this, clearInfoLastReceived: infoTimeStamp == null, infoLastReceived: infoTimeStamp);



		/// <summary>
		///     Creates a copy of this tracker with an updated tracker name value
		/// </summary>
		[Pure]
		public PsnTracker WithTrackerName([NotNull] string trackerName) => Clone(this, trackerName: trackerName);

		/// <summary>
		///     Creates a copy of this tracker with an updated position value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker WithPosition([CanBeNull] Tuple<float, float, float> position)
			=> Clone(this, clearPosition: position == null, position: position);

		/// <summary>
		///     Creates a copy of this tracker with an updated speed value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker WithSpeed([CanBeNull] Tuple<float, float, float> speed)
			=> Clone(this, clearSpeed: speed == null, speed: speed);

		/// <summary>
		///     Creates a copy of this tracker with an updated orientation value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker WithOrientation([CanBeNull] Tuple<float, float, float> orientation)
			=> Clone(this, clearOrientation: orientation == null, orientation: orientation);

		/// <summary>
		///     Creates a copy of this tracker with an updated acceleration value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker WithAcceleration([CanBeNull] Tuple<float, float, float> acceleration)
			=> Clone(this, clearAcceleration: acceleration == null, acceleration: acceleration);

		/// <summary>
		///     Creates a copy of this tracker with an updated target position value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker WithTargetPosition([CanBeNull] Tuple<float, float, float> targetPosition)
			=> Clone(this, clearTargetPosition: targetPosition == null, targetPosition: targetPosition);

        /// <summary>
        ///     Creates a copy of this tracker with an updated timestamp value, or clears value if null
        /// </summary>
        [Pure]
        public PsnTracker WithTargetPosition([CanBeNull] ulong? timestamp)
            => Clone(this, clearTimestamp: timestamp == null, timestamp: timestamp);

		/// <summary>
		///     Creates a copy of this tracker with an updated validity value, or clears value if null
		/// </summary>
		[Pure]
		public PsnTracker WithValidity([CanBeNull] float? validity)
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
				yield return
					new PsnDataTrackerTrgtPosChunk(TargetPosition.Item1, TargetPosition.Item2, TargetPosition.Item3);

            if (Timestamp != null)
                yield return new PsnDataTrackerTimestampChunk(Timestamp.Value);

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

		/// <inheritdoc/>
		public bool Equals(PsnTracker other)
		{
			return TrackerId == other.TrackerId && string.Equals(TrackerName, other.TrackerName)
				   && Equals(Position, other.Position) && Equals(Speed, other.Speed)
				   && Equals(Orientation, other.Orientation)
				   && Equals(Acceleration, other.Acceleration) && Equals(TargetPosition, other.TargetPosition)
				   && Validity.Equals(other.Validity);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is PsnTracker tracker && Equals(tracker);
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public override string ToString()
		{
			return
				$"PsnTracker: Id {TrackerId}" +
				", Name " + (TrackerName ?? "(Unknown)") +
				", InfoLastReceived "
				+ (InfoLastReceived.HasValue ? TimeSpan.FromMilliseconds(InfoLastReceived.Value).ToString() : "None") +
				", DataLastReceived "
				+ (DataLastReceived.HasValue ? TimeSpan.FromMilliseconds(DataLastReceived.Value).ToString() : "None") +
				(Position != null ? $", Position {Position}" : string.Empty) +
				(Speed != null ? $", Speed {Speed}" : string.Empty) +
				(Orientation != null ? $", Orientation {Orientation}" : string.Empty) +
				(Acceleration != null ? $", Acceleration {Acceleration}" : string.Empty) +
				(TargetPosition != null ? $", Target Position {TargetPosition}" : string.Empty) +
				(Timestamp != null ? $", Timestamp {TargetPosition}" : string.Empty) +
				(Validity != null ? $", Validity {Validity}" : string.Empty);
		}
	}
}