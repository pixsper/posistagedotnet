using System;
using System.Collections.Generic;
using Imp.PosiStageDotNet.Serialization;

namespace Imp.PosiStageDotNet.Chunks
{
	internal class PsnDataTrackerListChunk : PsnChunk
	{
		public PsnDataTrackerListChunk(IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
		{
			
		}

		public override ushort ChunkId => (ushort)PsnDataChunkId.PsnDataTrackerList;
		public override int DataLength => 0;
	}




	internal class PsnDataTrackerChunk : PsnChunk
	{
		public PsnDataTrackerChunk(int trackerId, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId, $"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			ChunkId = (ushort)trackerId;
		}

		public override ushort ChunkId { get; }
		public override int DataLength => 0;
	}




	internal class PsnDataTrackerPosChunk : PsnChunk
	{
		public PsnDataTrackerPosChunk(float x, float y, float z, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	internal class PsnDataTrackerSpeedChunk : PsnChunk
	{
		public PsnDataTrackerSpeedChunk(float x, float y, float z, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	internal class PsnDataTrackerOriChunk : PsnChunk
	{
		public PsnDataTrackerOriChunk(float x, float y, float z, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	internal class PsnDataTrackerStatusChunk : PsnChunk
	{
		public PsnDataTrackerStatusChunk(float validity, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
		{
			Validity = validity;
		}

		public float Validity { get; }

		public override ushort ChunkId => (ushort)PsnDataTrackerChunkId.PsnDataTrackerStatus;
		public override int DataLength => 4;

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(Validity);
		}
	}


	internal class PsnDataTrackerAccelChunk : PsnChunk
	{
		public PsnDataTrackerAccelChunk(float x, float y, float z, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}


	internal class PsnDataTrackerTrgtPosChunk : PsnChunk
	{
		public PsnDataTrackerTrgtPosChunk(float x, float y, float z, IEnumerable<PsnChunk> subChunks = null)
			: base(subChunks)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}
}
