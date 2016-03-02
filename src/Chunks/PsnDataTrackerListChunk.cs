using System;
using System.Collections.Generic;
using System.IO;
using Imp.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace Imp.PosiStageDotNet.Chunks
{
	internal class PsnDataTrackerListChunk : PsnChunk
	{
		public static PsnDataTrackerListChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
		{
			var subChunks = new List<PsnChunk>();

			foreach (var pair in FindSubChunkHeaders(reader, chunkHeader.DataLength))
			{
				reader.Seek(pair.Item2, SeekOrigin.Begin);
				subChunks.Add(PsnDataTrackerChunk.Deserialize(pair.Item1, reader));
			}

			return new PsnDataTrackerListChunk(subChunks);
		}

		public PsnDataTrackerListChunk([NotNull] IEnumerable<PsnChunk> subChunks) : base(subChunks) { }

		public PsnDataTrackerListChunk(params PsnChunk[] subChunks) : base(subChunks) { }

		public override ushort ChunkId => (ushort)PsnDataChunkId.PsnDataTrackerList;
		public override int DataLength => 0;
	}



	internal class PsnDataTrackerChunk : PsnChunk
	{
		public static PsnDataTrackerChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		public PsnDataTrackerChunk(int trackerId, [NotNull] IEnumerable<PsnChunk> subChunks)
			: base(subChunks)
		{
			if (trackerId < ushort.MinValue || trackerId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(trackerId), trackerId,
					$"trackerId must be in the range {ushort.MinValue}-{ushort.MaxValue}");

			ChunkId = (ushort)trackerId;
		}

		public PsnDataTrackerChunk(int trackerId, params PsnChunk[] subChunks)
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



	internal class PsnDataTrackerPosChunk : PsnChunk
	{
		public static PsnDataTrackerPosChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}



	internal class PsnDataTrackerSpeedChunk : PsnChunk
	{
		public static PsnDataTrackerSpeedChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}



	internal class PsnDataTrackerOriChunk : PsnChunk
	{
		public static PsnDataTrackerOriChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}



	internal class PsnDataTrackerStatusChunk : PsnChunk
	{
		public static PsnDataTrackerStatusChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(Validity);
		}
	}



	internal class PsnDataTrackerAccelChunk : PsnChunk
	{
		public static PsnDataTrackerAccelChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}



	internal class PsnDataTrackerTrgtPosChunk : PsnChunk
	{
		public static PsnDataTrackerTrgtPosChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
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

		protected override void SerializeData(PsnBinaryWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}
	}
}