using System;
using System.Linq;
using System.Xml.Linq;
using DBDesign.PosiStageDotNet.Serialization;
using JetBrains.Annotations;

namespace DBDesign.PosiStageDotNet.Chunks
{
    /// <summary>
    ///     Represents a PosiStageNet chunk which was unable to be deserialized as it's type is unknown
    /// </summary>
    [PublicAPI]
    public sealed class PsnUnknownChunk : PsnChunk, IEquatable<PsnUnknownChunk>
    {
        internal PsnUnknownChunk(ushort rawChunkId, [CanBeNull] byte[] data)
            : base(null)
        {
            RawChunkId = rawChunkId;
            Data = data ?? new byte[0];
        }

        /// <summary>
        ///     Unserialized data contained in this chunk
        /// </summary>
        public byte[] Data { get; }

        /// <inheritdoc/>
        public override ushort RawChunkId { get; }

        /// <inheritdoc/>
        public override int DataLength => 0;


        /// <inheritdoc/>
        public bool Equals(PsnUnknownChunk other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return base.Equals(other) && Data.SequenceEqual(other.Data);
        }

        /// <inheritdoc/>
        public override XElement ToXml()
        {
            return new XElement(nameof(PsnUnknownChunk),
                new XAttribute("DataLength", Data.Length));
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((PsnUnknownChunk)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Data.GetHashCode();
                hashCode = (hashCode * 397) ^ RawChunkId.GetHashCode();
                return hashCode;
            }
        }

        internal static PsnUnknownChunk Deserialize(PsnChunkHeader chunkHeader, PsnBinaryReader reader)
        {
            // We can't proceed to deserialize any chunks from this point so store the raw data including sub-chunks
            return new PsnUnknownChunk(chunkHeader.ChunkId, reader.ReadBytes(chunkHeader.DataLength));
        }
    }
}