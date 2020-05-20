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
using System.Xml.Linq;
using DBDesign.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace DBDesign.PosiStageDotNet.Chunks
{
	/// <summary>
	///     PosiStageNet data packet chunk containing a list of data trackers
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerListChunk : PsnDataPacketSubChunk
	{
		/// <summary>
		///		Data tracker list chunk constructor
		/// </summary>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnDataTrackerListChunk([NotNull] IEnumerable<PsnDataTrackerChunk> subChunks)
			: this((IEnumerable<PsnChunk>)subChunks) { }

		/// <summary>
		///		Data tracker list chunk constructor
		/// </summary>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnDataTrackerListChunk(params PsnDataTrackerChunk[] subChunks) : this((IEnumerable<PsnChunk>)subChunks) { }

		private PsnDataTrackerListChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		/// <inheritdoc/>
		public override int DataLength => 0;

		/// <inheritdoc/>
		public override PsnDataPacketChunkId ChunkId => PsnDataPacketChunkId.PsnDataTrackerList;

		/// <summary>
		///		Typed sub-chunks of this chunk
		/// </summary>
		public IEnumerable<PsnDataTrackerChunk> SubChunks => RawSubChunks.OfType<PsnDataTrackerChunk>();

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerListChunk),
				RawSubChunks.Select(c => c.ToXml()));
		}

		internal static PsnDataTrackerListChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnDataTrackerChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);
				subChunks.Add(PsnDataTrackerChunk.Deserialize(pair.Item1, reader));
			}

			return new PsnDataTrackerListChunk(subChunks);
		}
	}


	/// <summary>
	///		PosiStageNet data packet chunk containing data for one data tracker. Only valid as sub-chunk of a <see cref="PsnDataTrackerListChunk"/>.
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerChunk : PsnChunk, IEquatable<PsnDataTrackerChunk>
	{
		/// <summary>
		///		Data tracker chunk constructor
		/// </summary>
		/// <param name="trackerId">ID of this tracker</param>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnDataTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnDataTrackerSubChunk> subChunks)
			: this(trackerId, (IEnumerable<PsnChunk>)subChunks) { }

		/// <summary>
		///		Data tracker chunk constructor
		/// </summary>
		/// <param name="trackerId">ID of this tracker</param>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		public PsnDataTrackerChunk(int trackerId, params PsnDataTrackerSubChunk[] subChunks)
			: this(trackerId, (IEnumerable<PsnChunk>)subChunks) { }

		private PsnDataTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnChunk> subChunks)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			RawChunkId = (ushort)trackerId;
		}

		/// <summary><inheritdoc/></summary>
		/// <remarks>
		///		Trackers have no specific chunk ID and use this value to store the tracker ID
		/// </remarks>
		public override ushort RawChunkId { get; }

		/// <inheritdoc/>
		public override int DataLength => 0;

		/// <summary>
		///		ID of this tracker, stored in <see cref="RawChunkId"/>
		/// </summary>
		public int TrackerId => RawChunkId;

		/// <summary>
		///		Typed sub-chunks of this chunk
		/// </summary>
		public IEnumerable<PsnDataTrackerSubChunk> SubChunks => RawSubChunks.OfType<PsnDataTrackerSubChunk>();

		/// <inheritdoc/>
		public bool Equals(PsnDataTrackerChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && RawChunkId == other.RawChunkId;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ RawChunkId.GetHashCode();
			}
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerChunk),
				new XAttribute("TrackerId", RawChunkId),
				RawSubChunks.Select(c => c.ToXml()));
		}

		internal static PsnDataTrackerChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);

				switch ((PsnDataTrackerChunkId)pair.Item1.ChunkId)
				{
					case PsnDataTrackerChunkId.PsnDataTrackerPos:
						subChunks.Add(PsnDataTrackerPosChunk.Deserialize(chunkHeader, reader));
						break;
					case PsnDataTrackerChunkId.PsnDataTrackerSpeed:
						subChunks.Add(PsnDataTrackerSpeedChunk.Deserialize(chunkHeader, reader));
						break;
					case PsnDataTrackerChunkId.PsnDataTrackerOri:
						subChunks.Add(PsnDataTrackerOriChunk.Deserialize(chunkHeader, reader));
						break;
					case PsnDataTrackerChunkId.PsnDataTrackerStatus:
						subChunks.Add(PsnDataTrackerStatusChunk.Deserialize(chunkHeader, reader));
						break;
					case PsnDataTrackerChunkId.PsnDataTrackerAccel:
						subChunks.Add(PsnDataTrackerAccelChunk.Deserialize(chunkHeader, reader));
						break;
					case PsnDataTrackerChunkId.PsnDataTrackerTrgtPos:
						subChunks.Add(PsnDataTrackerTrgtPosChunk.Deserialize(chunkHeader, reader));
						break;
					default:
						subChunks.Add(PsnUnknownChunk.Deserialize(chunkHeader, reader));
						break;
				}
			}

			return new PsnDataTrackerChunk(chunkHeader.ChunkId, subChunks);
		}
	}


	/// <summary>
	///		Base class for sub-chunks of <see cref="PsnDataTrackerChunk"/>
	/// </summary>
	[PublicAPI]
	public abstract class PsnDataTrackerSubChunk : PsnChunk
	{
		/// <summary>
		///		Base data tracker sub-chunk constructor
		/// </summary>
		/// <param name="subChunks">Typed sub-chunks of this chunk</param>
		protected PsnDataTrackerSubChunk([CanBeNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		/// <summary>
		///		Typed chunk ID
		/// </summary>
		public abstract PsnDataTrackerChunkId ChunkId { get; }

		/// <inheritdoc/>
		public override ushort RawChunkId => (ushort)ChunkId;
	}


	/// <summary>
	///		Data tracker sub-chunk containing tracker position
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerPosChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerPosChunk>
	{
		/// <summary>
		///		Tracker position chunk constructor
		/// </summary>
		/// <param name="x">X axis position in m</param>
		/// <param name="y">Y axis position in m</param>
		/// <param name="z">Z axis position in m</param>
		public PsnDataTrackerPosChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		///		X axis position in m
		/// </summary>
		public float X { get; }

		/// <summary>
		///		Y axis position in m
		/// </summary>
		public float Y { get; }

		/// <summary>
		///		Z axis position in m
		/// </summary>
		public float Z { get; }

		/// <summary>
		///		Axis position in m as vector
		/// </summary>
		public Tuple<float, float, float> Vector => Tuple.Create(X, Y, Z);

		/// <inheritdoc/>
		public override int DataLength => 12;

		/// <inheritdoc/>
		public override PsnDataTrackerChunkId ChunkId => PsnDataTrackerChunkId.PsnDataTrackerPos;

		/// <inheritdoc/>
		public bool Equals(PsnDataTrackerPosChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerPosChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerPosChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}

		internal static PsnDataTrackerPosChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerPosChunk(x, y, z);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	/// <summary>
	///		Data tracker sub-chunk containing tracker speed
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerSpeedChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerSpeedChunk>
	{
		/// <summary>
		///		Tracker speed chunk constructor
		/// </summary>
		/// <param name="x">X axis speed in m/s</param>
		/// <param name="y">Y axis speed in m/s</param>
		/// <param name="z">Z axis speed in m/s</param>
		public PsnDataTrackerSpeedChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		///		X axis speed in m/s
		/// </summary>
		public float X { get; }

		/// <summary>
		///		Y axis speed in m/s
		/// </summary>
		public float Y { get; }

		/// <summary>
		///		Z axis speed in m/s
		/// </summary>
		public float Z { get; }

		/// <summary>
		///		Axis speed in m/s as vector
		/// </summary>
		public Tuple<float, float, float> Vector => Tuple.Create(X, Y, Z);

		/// <inheritdoc/>
		public override int DataLength => 12;

		/// <inheritdoc/>
		public override PsnDataTrackerChunkId ChunkId => PsnDataTrackerChunkId.PsnDataTrackerSpeed;

		/// <inheritdoc/>
		public bool Equals(PsnDataTrackerSpeedChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerSpeedChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerSpeedChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}

		internal static PsnDataTrackerSpeedChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerSpeedChunk(x, y, z);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	/// <summary>
	///		Data tracker sub-chunk containing tracker orientation
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerOriChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerOriChunk>
	{
		/// <summary>
		///		Tracker orientation chunk constructor
		/// </summary>
		/// <param name="x">X component of vector indicating absolute orientation around rotation axis in radians</param>
		/// <param name="y">Y component of vector indicating absolute orientation around rotation axis in radians</param>
		/// <param name="z">Z component of vector indicating absolute orientation around rotation axis in radians</param>
		public PsnDataTrackerOriChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		///		X component of vector indicating absolute orientation around rotation axis in radians
		/// </summary>
		public float X { get; }

		/// <summary>
		///		Y component of vector indicating absolute orientation around rotation axis in radians
		/// </summary>
		public float Y { get; }

		/// <summary>
		///		Z component of vector indicating absolute orientation around rotation axis in radians
		/// </summary>
		public float Z { get; }

		/// <summary>
		///		Vector indicating absolute orientation around rotation axis in radians
		/// </summary>
		public Tuple<float, float, float> Vector => Tuple.Create(X, Y, Z);

		/// <inheritdoc/>
		public override int DataLength => 12;

		/// <inheritdoc/>
		public override PsnDataTrackerChunkId ChunkId => PsnDataTrackerChunkId.PsnDataTrackerOri;

		/// <inheritdoc/>
		public bool Equals(PsnDataTrackerOriChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerOriChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerOriChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}

		internal static PsnDataTrackerOriChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerOriChunk(x, y, z);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	/// <summary>
	///		Data tracker sub-chunk containing tracker status
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerStatusChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerStatusChunk>
	{
		/// <summary>
		///		Tracker status chunk constructor
		/// </summary>
		/// <param name="validity">Validity of tracker data</param>
		public PsnDataTrackerStatusChunk(float validity)
			: base(null)
		{
			Validity = validity;
		}

		/// <summary>
		///		Validity value for tracker
		/// </summary>
		public float Validity { get; }

		/// <inheritdoc/>
		public override int DataLength => 4;

		/// <inheritdoc/>
		public override PsnDataTrackerChunkId ChunkId => PsnDataTrackerChunkId.PsnDataTrackerStatus;

		/// <inheritdoc/>
		public bool Equals(PsnDataTrackerStatusChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && Validity.Equals(other.Validity);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerStatusChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ Validity.GetHashCode();
			}
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerStatusChunk),
				new XAttribute(nameof(Validity), Validity));
		}

		internal static PsnDataTrackerStatusChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float validity = reader.ReadSingle();

			return new PsnDataTrackerStatusChunk(validity);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(Validity);
		}
	}


	/// <summary>
	///		Data tracker sub-chunk containing tracker acceleration
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerAccelChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerAccelChunk>
	{
		/// <summary>
		///		Tracker acceleration chunk constructor 
		/// </summary>
		/// <param name="x">X axis acceleration in m/s^2</param>
		/// <param name="y">Y axis acceleration in m/s^2</param>
		/// <param name="z">Z axis acceleration in m/s^2</param>
		public PsnDataTrackerAccelChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		///		X axis acceleration in m/s^2
		/// </summary>
		public float X { get; }

		/// <summary>
		///		Y axis acceleration in m/s^2
		/// </summary>
		public float Y { get; }

		/// <summary>
		///		Z axis acceleration in m/s^2
		/// </summary>
		public float Z { get; }

		/// <summary>
		///		Axis acceleration in m/s^2 as vector
		/// </summary>
		public Tuple<float, float, float> Vector => Tuple.Create(X, Y, Z);

		/// <inheritdoc/>
		public override int DataLength => 12;

		/// <inheritdoc/>
		public override PsnDataTrackerChunkId ChunkId => PsnDataTrackerChunkId.PsnDataTrackerAccel;

		/// <inheritdoc/>
		public bool Equals(PsnDataTrackerAccelChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerAccelChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerAccelChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}

		internal static PsnDataTrackerAccelChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerAccelChunk(x, y, z);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	/// <summary>
	///		Data tracker sub-chunk containing tracker target position
	/// </summary>
	[PublicAPI]
	public sealed class PsnDataTrackerTrgtPosChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerTrgtPosChunk>
	{
		/// <summary>
		///		Tracker target position chunk constructor
		/// </summary>
		/// <param name="x">X axis position in m</param>
		/// <param name="y">Y axis position in m</param>
		/// <param name="z">Z axis position in m</param>
		public PsnDataTrackerTrgtPosChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		///		X axis target position in m
		/// </summary>
		public float X { get; }

		/// <summary>
		///		Y axis target position in m
		/// </summary>
		public float Y { get; }

		/// <summary>
		///		Z axis target position in m
		/// </summary>
		public float Z { get; }

		/// <summary>
		///		Axis target position in m as vector
		/// </summary>
		public Tuple<float, float, float> Vector => Tuple.Create(X, Y, Z);

		/// <inheritdoc/>
		public override int DataLength => 12;

		/// <inheritdoc/>
		public override PsnDataTrackerChunkId ChunkId => PsnDataTrackerChunkId.PsnDataTrackerTrgtPos;

		/// <inheritdoc/>
		public bool Equals(PsnDataTrackerTrgtPosChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerTrgtPosChunk)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerTrgtPosChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}

		internal static PsnDataTrackerTrgtPosChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerTrgtPosChunk(x, y, z);
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}
}