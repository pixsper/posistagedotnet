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
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	[PublicAPI]
	public sealed class PsnDataTrackerListChunk : PsnDataPacketSubChunk
	{
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

		public PsnDataTrackerListChunk([NotNull] IEnumerable<PsnDataTrackerChunk> subChunks)
			: this((IEnumerable<PsnChunk>)subChunks) { }

		public PsnDataTrackerListChunk(params PsnDataTrackerChunk[] subChunks) : this((IEnumerable<PsnChunk>)subChunks) { }

		private PsnDataTrackerListChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public override ushort ChunkId => (ushort)PsnDataChunkId.PsnDataTrackerList;
		public override int DataLength => 0;

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerListChunk),
				SubChunks.Select(c => c.ToXml()));
		}
	}



	[PublicAPI]
	public sealed class PsnDataTrackerChunk : PsnChunk, IEquatable<PsnDataTrackerChunk>
	{
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

		public PsnDataTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnDataTrackerSubChunk> subChunks)
			: this(trackerId, (IEnumerable<PsnChunk>)subChunks) { }

		public PsnDataTrackerChunk(int trackerId, params PsnDataTrackerSubChunk[] subChunks)
			: this(trackerId, (IEnumerable<PsnChunk>)subChunks) { }

		private PsnDataTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnChunk> subChunks)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			ChunkId = (ushort)trackerId;
		}

		public override ushort ChunkId { get; }
		public override int DataLength => 0;

		public bool Equals([CanBeNull] PsnDataTrackerChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && ChunkId == other.ChunkId;
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerChunk)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ ChunkId.GetHashCode();
			}
		}

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerChunk),
				new XAttribute("TrackerId", ChunkId),
				SubChunks.Select(c => c.ToXml()));
		}
	}



	[PublicAPI]
	public abstract class PsnDataTrackerSubChunk : PsnChunk
	{
		protected PsnDataTrackerSubChunk([CanBeNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }
	}



	[PublicAPI]
	public sealed class PsnDataTrackerPosChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerPosChunk>
	{
		internal static PsnDataTrackerPosChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerPosChunk(x, y, z);
		}

		public PsnDataTrackerPosChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; }
		public float Y { get; }
		public float Z { get; }

		public override ushort ChunkId => (ushort)PsnDataTrackerChunkId.PsnDataTrackerPos;
		public override int DataLength => 12;

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}

		public bool Equals([CanBeNull] PsnDataTrackerPosChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerPosChunk)obj);
		}

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

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerPosChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}
	}



	[PublicAPI]
	public sealed class PsnDataTrackerSpeedChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerSpeedChunk>
	{
		internal static PsnDataTrackerSpeedChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerSpeedChunk(x, y, z);
		}

		public PsnDataTrackerSpeedChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; }
		public float Y { get; }
		public float Z { get; }

		public override ushort ChunkId => (ushort)PsnDataTrackerChunkId.PsnDataTrackerSpeed;
		public override int DataLength => 12;

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}

		public bool Equals([CanBeNull] PsnDataTrackerSpeedChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerSpeedChunk)obj);
		}

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

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerSpeedChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}
	}



	[PublicAPI]
	public sealed class PsnDataTrackerOriChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerOriChunk>
	{
		internal static PsnDataTrackerOriChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerOriChunk(x, y, z);
		}

		public PsnDataTrackerOriChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; }
		public float Y { get; }
		public float Z { get; }

		public override ushort ChunkId => (ushort)PsnDataTrackerChunkId.PsnDataTrackerOri;
		public override int DataLength => 12;

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}

		public bool Equals([CanBeNull] PsnDataTrackerOriChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerOriChunk)obj);
		}

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

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerOriChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}
	}



	[PublicAPI]
	public sealed class PsnDataTrackerStatusChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerStatusChunk>
	{
		internal static PsnDataTrackerStatusChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float validity = reader.ReadSingle();

			return new PsnDataTrackerStatusChunk(validity);
		}

		public PsnDataTrackerStatusChunk(float validity)
			: base(null)
		{
			Validity = validity;
		}

		public float Validity { get; }

		public override ushort ChunkId => (ushort)PsnDataTrackerChunkId.PsnDataTrackerStatus;
		public override int DataLength => 4;

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(Validity);
		}

		public bool Equals([CanBeNull] PsnDataTrackerStatusChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && Validity.Equals(other.Validity);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerStatusChunk)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ Validity.GetHashCode();
			}
		}

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerStatusChunk),
				new XAttribute(nameof(Validity), Validity));
		}
	}



	[PublicAPI]
	public sealed class PsnDataTrackerAccelChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerAccelChunk>
	{
		internal static PsnDataTrackerAccelChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerAccelChunk(x, y, z);
		}

		public PsnDataTrackerAccelChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; }
		public float Y { get; }
		public float Z { get; }

		public override ushort ChunkId => (ushort)PsnDataTrackerChunkId.PsnDataTrackerAccel;
		public override int DataLength => 12;

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}

		public bool Equals([CanBeNull] PsnDataTrackerAccelChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerAccelChunk)obj);
		}

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

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerAccelChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}
	}



	[PublicAPI]
	public sealed class PsnDataTrackerTrgtPosChunk : PsnDataTrackerSubChunk, IEquatable<PsnDataTrackerTrgtPosChunk>
	{
		internal static PsnDataTrackerTrgtPosChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();

			return new PsnDataTrackerTrgtPosChunk(x, y, z);
		}

		public PsnDataTrackerTrgtPosChunk(float x, float y, float z)
			: base(null)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; }
		public float Y { get; }
		public float Z { get; }

		public override ushort ChunkId => (ushort)PsnDataTrackerChunkId.PsnDataTrackerTrgtPos;
		public override int DataLength => 12;

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}

		public bool Equals([CanBeNull] PsnDataTrackerTrgtPosChunk other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnDataTrackerTrgtPosChunk)obj);
		}

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

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnDataTrackerTrgtPosChunk),
				new XAttribute(nameof(X), X),
				new XAttribute(nameof(Y), Y),
				new XAttribute(nameof(Z), Z));
		}
	}
}