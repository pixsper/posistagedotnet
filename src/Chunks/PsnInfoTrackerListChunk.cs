using System;
using System.Collections.Generic;
using Imp.PosiStageDotNet.Serialization;

namespace Imp.PosiStageDotNet.Chunks
{

	internal class PsnInfoTrackerListChunk : PsnChunk
	{
		public PsnInfoTrackerListChunk(IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
		{
			
		}

		public override ushort ChunkId => (ushort)PsnInfoChunkId.PsnInfoTrackerList;
		public override int DataLength => 0;
	}




	internal class PsnInfoTracker : PsnChunk
	{
		public PsnInfoTracker(int trackerId, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId, $"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			ChunkId = (ushort)trackerId;
		}

		public override ushort ChunkId { get; }
		public override int DataLength => 0;
	}




	internal class PsnInfoTrackerName : PsnChunk
	{
		public PsnInfoTrackerName([NotNull] string trackerName, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
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
