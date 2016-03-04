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
	public sealed class PsnInfoTrackerListChunk : PsnInfoPacketSubChunk
	{
		public PsnInfoTrackerListChunk([NotNull] IEnumerable<PsnInfoTrackerChunk> subChunks)
			: this((IEnumerable<PsnChunk>)subChunks) { }

		public PsnInfoTrackerListChunk(params PsnInfoTrackerChunk[] subChunks) : this((IEnumerable<PsnChunk>)subChunks) { }

		public PsnInfoTrackerListChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public override PsnInfoPacketChunkId ChunkId => PsnInfoPacketChunkId.PsnInfoTrackerList;
		public override int DataLength => 0;

		public IEnumerable<PsnInfoTrackerChunk> SubChunks => RawSubChunks.OfType<PsnInfoTrackerChunk>();

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnInfoTrackerListChunk),
				RawSubChunks.Select(c => c.ToXml()));
		}

		internal static PsnInfoTrackerListChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);
				subChunks.Add(PsnInfoTrackerChunk.Deserialize(pair.Item1, reader));
			}

			return new PsnInfoTrackerListChunk(subChunks);
		}
	}



	[PublicAPI]
	public class PsnInfoTrackerChunk : PsnChunk
	{
		public PsnInfoTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnInfoTrackerSubChunk> subChunks)
			: this(trackerId, (IEnumerable<PsnChunk>)subChunks) { }

		public PsnInfoTrackerChunk(int trackerId, params PsnInfoTrackerSubChunk[] subChunks)
			: this(trackerId, (IEnumerable<PsnChunk>)subChunks) { }

		private PsnInfoTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnChunk> subChunks)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			RawChunkId = (ushort)trackerId;
		}

		public override ushort RawChunkId { get; }
		public override int DataLength => 0;

		public int TrackerId => RawChunkId;

		public IEnumerable<PsnInfoTrackerSubChunk> SubChunks => RawSubChunks.OfType<PsnInfoTrackerSubChunk>();

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnInfoTrackerChunk),
				new XAttribute("TrackerId", RawChunkId),
				RawSubChunks.Select(c => c.ToXml()));
		}

		internal static PsnInfoTrackerChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);

				switch ((PsnInfoTrackerChunkId)pair.Item1.ChunkId)
				{
					case PsnInfoTrackerChunkId.PsnInfoTrackerName:
						subChunks.Add(PsnInfoTrackerName.Deserialize(pair.Item1, reader));
						break;
					default:
						subChunks.Add(PsnUnknownChunk.Deserialize(chunkHeader, reader));
						break;
				}
			}

			return new PsnInfoTrackerChunk(chunkHeader.ChunkId, subChunks);
		}
	}



	[PublicAPI]
	public abstract class PsnInfoTrackerSubChunk : PsnChunk
	{
		protected PsnInfoTrackerSubChunk([CanBeNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public abstract PsnInfoTrackerChunkId ChunkId { get; }
		public override ushort RawChunkId => (ushort)ChunkId;
	}



	[PublicAPI]
	public class PsnInfoTrackerName : PsnInfoTrackerSubChunk, IEquatable<PsnInfoTrackerName>
	{
		public PsnInfoTrackerName([NotNull] string trackerName)
			: base(null)
		{
			if (trackerName == null)
				throw new ArgumentException(nameof(trackerName));

			TrackerName = trackerName;
		}

		public string TrackerName { get; }

		public override int DataLength => TrackerName.Length;

		public override PsnInfoTrackerChunkId ChunkId => PsnInfoTrackerChunkId.PsnInfoTrackerName;

		public bool Equals([CanBeNull] PsnInfoTrackerName other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && string.Equals(TrackerName, other.TrackerName);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((PsnInfoTrackerName)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ TrackerName.GetHashCode();
			}
		}

		public override XElement ToXml()
		{
			return new XElement(nameof(PsnInfoTrackerName),
				new XAttribute(nameof(TrackerName), TrackerName));
		}

		internal static PsnInfoTrackerName Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			return new PsnInfoTrackerName(reader.ReadString(chunkHeader.DataLength));
		}

		internal override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(TrackerName);
		}
	}
}