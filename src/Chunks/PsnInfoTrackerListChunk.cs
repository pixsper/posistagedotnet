using System;
using System.Collections.Generic;
using System.IO;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	internal class PsnInfoTrackerListChunk : PsnChunk
	{
		public static PsnInfoTrackerListChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);
				subChunks.Add(PsnInfoTrackerChunk.Deserialize(pair.Item1, reader));
			}

			return new PsnInfoTrackerListChunk(subChunks);
		}

		public PsnInfoTrackerListChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public PsnInfoTrackerListChunk(params PsnChunk[] subChunks) : base(subChunks) { }

		public override ushort ChunkId => (ushort)PsnInfoChunkId.PsnInfoTrackerList;
		public override int DataLength => 0;
	}



	internal class PsnInfoTrackerChunk : PsnChunk
	{
		public static PsnInfoTrackerChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		public PsnInfoTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnChunk> subChunks)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			ChunkId = (ushort)trackerId;
		}

		public PsnInfoTrackerChunk(int trackerId, params PsnChunk[] subChunks)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			ChunkId = (ushort)trackerId;
		}

		public override ushort ChunkId { get; }
		public override int DataLength => 0;
	}



	internal class PsnInfoTrackerName : PsnChunk
	{
		public static PsnInfoTrackerName Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			string trackerName = reader.ReadString();
			return new PsnInfoTrackerName(trackerName);
		}

		public PsnInfoTrackerName([NotNull] string trackerName)
			: base(null)
		{
			if (trackerName == null)
				throw new ArgumentException(nameof(trackerName));

			TrackerName = trackerName;
		}

		public string TrackerName { get; }

		public override ushort ChunkId => (ushort)PsnInfoTrackerChunkId.PsnInfoTrackerName;
		public override int DataLength => TrackerName.Length;

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(TrackerName);
		}
	}
}