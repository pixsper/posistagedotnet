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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Imp.PosiStageDotNet.Serialization
{
    // Note: Based on code from MiscUtil r285 February 26th 2009 - http://www.yoda.arachsys.com/csharp/miscutil/

    /// <summary>
    ///     Equivalent of <see cref="BinaryReader" />, but with either endianness, depending on
    ///     the EndianBitConverter it is constructed with. No data is buffered in the
    ///     reader; the client may seek within the stream at will.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverriden.Global")]
    internal class EndianBinaryReader : IDisposable
    {
        /// <summary>
        ///     Buffer used for temporary storage before conversion into primitives
        /// </summary>
        private readonly byte[] _buffer = new byte[16];

        /// <summary>
        ///     Buffer used for temporary storage when reading a single character
        /// </summary>
        private readonly char[] _charBuffer = new char[1];

        /// <summary>
        ///     Decoder to use for string conversions.
        /// </summary>
        readonly Decoder _decoder;

        /// <summary>
        ///     Minimum number of bytes used to encode a character
        /// </summary>
        private readonly int _minBytesPerChar;

        /// <summary>
        ///     Whether or not this reader has been _disposed yet.
        /// </summary>
        bool _isDisposed;


        /// <summary>
        ///     Equivalent of <see cref="BinaryWriter" />, but with either endianness, depending on
        ///     the <see cref="EndianBitConverter" /> it is constructed with.
        /// </summary>
        /// <param name="bitConverter">Converter to use when reading data</param>
        /// <param name="stream">Stream to read data from</param>
        public EndianBinaryReader(EndianBitConverter bitConverter, Stream stream)
            : this(bitConverter, stream, Encoding.UTF8) { }

        /// <summary>
        ///     Constructs a new binary reader with the given bit converter, reading
        ///     to the given stream, using the given encoding.
        /// </summary>
        /// <param name="bitConverter">Converter to use when reading data</param>
        /// <param name="stream">Stream to read data from</param>
        /// <param name="encoding">Encoding to use when reading character data</param>
        /// <exception cref="ArgumentException">Stream isn't writable</exception>
        public EndianBinaryReader(EndianBitConverter bitConverter, Stream stream, Encoding encoding)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream isn't writable", nameof(stream));

            BaseStream = stream;
            BitConverter = bitConverter;
            Encoding = encoding;
            _decoder = encoding.GetDecoder();
            _minBytesPerChar = 1;

            if (encoding is UnicodeEncoding)
                _minBytesPerChar = 2;
        }



        /// <summary>
        ///     The bit converter used to read values from the stream
        /// </summary>
        public EndianBitConverter BitConverter { get; }

        /// <summary>
        ///     The encoding used to read strings
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        ///     Gets the underlying stream of the EndianBinaryReader.
        /// </summary>
        public Stream BaseStream { get; }


        /// <summary>
        ///     Disposes of the underlying stream.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            ((IDisposable)BaseStream).Dispose();
        }



        /// <summary>
        ///     Closes the reader, including the underlying stream..
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        ///     Seeks within the stream.
        /// </summary>
        /// <param name="offset">Offset to seek to.</param>
        /// <param name="origin">Origin of seek operation.</param>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        public virtual void Seek(long offset, SeekOrigin origin)
        {
            checkDisposed();
            BaseStream.Seek(offset, origin);
        }

        /// <summary>
        ///     Reads a single byte from the stream.
        /// </summary>
        /// <returns>The byte read</returns>
        public virtual byte ReadByte()
        {
            readInternal(_buffer, 1);
            return _buffer[0];
        }

        /// <summary>
        ///     Reads a single signed byte from the stream.
        /// </summary>
        /// <returns>The byte read</returns>
        public virtual sbyte ReadSByte()
        {
            readInternal(_buffer, 1);
            return unchecked((sbyte)_buffer[0]);
        }

        /// <summary>
        ///     Reads a boolean from the stream. 1 byte is read.
        /// </summary>
        /// <returns>The boolean read</returns>
        public virtual bool ReadBoolean()
        {
            readInternal(_buffer, 1);
            return BitConverter.ToBoolean(_buffer, 0);
        }

        /// <summary>
        ///     Reads a 16-bit signed integer from the stream, using the bit converter
        ///     for this reader. 2 bytes are read.
        /// </summary>
        /// <returns>The 16-bit integer read</returns>
        public virtual short ReadInt16()
        {
            readInternal(_buffer, 2);
            return BitConverter.ToInt16(_buffer, 0);
        }

        /// <summary>
        ///     Reads a 32-bit signed integer from the stream, using the bit converter
        ///     for this reader. 4 bytes are read.
        /// </summary>
        /// <returns>The 32-bit integer read</returns>
        public virtual int ReadInt32()
        {
            readInternal(_buffer, 4);
            return BitConverter.ToInt32(_buffer, 0);
        }

        /// <summary>
        ///     Reads a 64-bit signed integer from the stream, using the bit converter
        ///     for this reader. 8 bytes are read.
        /// </summary>
        /// <returns>The 64-bit integer read</returns>
        public virtual long ReadInt64()
        {
            readInternal(_buffer, 8);
            return BitConverter.ToInt64(_buffer, 0);
        }

        /// <summary>
        ///     Reads a 16-bit unsigned integer from the stream, using the bit converter
        ///     for this reader. 2 bytes are read.
        /// </summary>
        /// <returns>The 16-bit unsigned integer read</returns>
        public virtual ushort ReadUInt16()
        {
            readInternal(_buffer, 2);
            return BitConverter.ToUInt16(_buffer, 0);
        }

        /// <summary>
        ///     Reads a 32-bit unsigned integer from the stream, using the bit converter
        ///     for this reader. 4 bytes are read.
        /// </summary>
        /// <returns>The 32-bit unsigned integer read</returns>
        public virtual uint ReadUInt32()
        {
            readInternal(_buffer, 4);
            return BitConverter.ToUInt32(_buffer, 0);
        }

        /// <summary>
        ///     Reads a 64-bit unsigned integer from the stream, using the bit converter
        ///     for this reader. 8 bytes are read.
        /// </summary>
        /// <returns>The 64-bit unsigned integer read</returns>
        public virtual ulong ReadUInt64()
        {
            readInternal(_buffer, 8);
            return BitConverter.ToUInt64(_buffer, 0);
        }

        /// <summary>
        ///     Reads a single-precision floating-point value from the stream, using the bit converter
        ///     for this reader. 4 bytes are read.
        /// </summary>
        /// <returns>The floating point value read</returns>
        public virtual float ReadSingle()
        {
            readInternal(_buffer, 4);
            return BitConverter.ToSingle(_buffer, 0);
        }

        /// <summary>
        ///     Reads a double-precision floating-point value from the stream, using the bit converter
        ///     for this reader. 8 bytes are read.
        /// </summary>
        /// <returns>The floating point value read</returns>
        public virtual double ReadDouble()
        {
            readInternal(_buffer, 8);
            return BitConverter.ToDouble(_buffer, 0);
        }

        /// <summary>
        ///     Reads a decimal value from the stream, using the bit converter
        ///     for this reader. 16 bytes are read.
        /// </summary>
        /// <returns>The decimal value read</returns>
        public virtual decimal ReadDecimal()
        {
            readInternal(_buffer, 16);
            return BitConverter.ToDecimal(_buffer, 0);
        }

        /// <summary>
        ///     Reads a single character from the stream, using the character encoding for
        ///     this reader. If no characters have been fully read by the time the stream ends,
        ///     -1 is returned.
        /// </summary>
        /// <returns>The character read, or -1 for end of stream.</returns>
        public virtual int Read()
        {
            int charsRead = Read(_charBuffer, 0, 1);
            if (charsRead == 0)
                return -1;

            return _charBuffer[0];
        }

        /// <summary>
        ///     Reads the specified number of characters into the given _buffer, starting at
        ///     the given index.
        /// </summary>
        /// <param name="data">The _buffer to copy data into</param>
        /// <param name="index">The first index to copy data into</param>
        /// <param name="count">The number of characters to read</param>
        /// <returns>
        ///     The number of characters actually read. This will only be less than
        ///     the requested number of characters if the end of the stream is reached.
        /// </returns>
        public virtual int Read(char[] data, int index, int count)
        {
            checkDisposed();

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count + index > data.Length)
                throw new ArgumentException(
                    "Not enough space in buffer for specified number of characters starting at specified index");

            int read = 0;
            bool firstTime = true;

            // Use the normal _buffer if we're only reading a small amount, otherwise
            // use at most 4K at a time.
            var byteBuffer = _buffer;

            if (byteBuffer.Length < count * _minBytesPerChar)
                byteBuffer = new byte[4096];

            while (read < count)
            {
                int amountToRead;
                // First time through we know we haven't previously read any data
                if (firstTime)
                {
                    amountToRead = count * _minBytesPerChar;
                    firstTime = false;
                }
                // After that we can only assume we need to fully read "chars left -1" characters
                // and a single byte of the character we may be in the middle of
                else
                {
                    amountToRead = (count - read - 1) * _minBytesPerChar + 1;
                }

                if (amountToRead > byteBuffer.Length)
                    amountToRead = byteBuffer.Length;

                int bytesRead = tryReadInternal(byteBuffer, amountToRead);
                if (bytesRead == 0)
                    return read;

                int decoded = _decoder.GetChars(byteBuffer, 0, bytesRead, data, index);
                read += decoded;
                index += decoded;
            }

            return read;
        }

        /// <summary>
        ///     Reads the specified number of bytes into the given _buffer, starting at
        ///     the given index.
        /// </summary>
        /// <param name="buffer">The _buffer to copy data into</param>
        /// <param name="index">The first index to copy data into</param>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>
        ///     The number of bytes actually read. This will only be less than
        ///     the requested number of bytes if the end of the stream is reached.
        /// </returns>
        public virtual int Read(byte[] buffer, int index, int count)
        {
            checkDisposed();

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count + index > buffer.Length)
                throw new ArgumentException(
                    "Not enough space in buffer for specified number of bytes starting at specified index");

            int read = 0;
            while (count > 0)
            {
                int block = BaseStream.Read(buffer, index, count);
                if (block == 0)
                    return read;

                index += block;
                read += block;
                count -= block;
            }

            return read;
        }

        /// <summary>
        ///     Reads the specified number of bytes, returning them in a new byte array.
        ///     If not enough bytes are available before the end of the stream, this
        ///     method will return what is available.
        /// </summary>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>The bytes read</returns>
        public virtual byte[] ReadBytes(int count)
        {
            checkDisposed();

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var ret = new byte[count];
            int index = 0;
            while (index < count)
            {
                int read = BaseStream.Read(ret, index, count - index);
                // Stream has finished half way through. That's fine, return what we've got.
                if (read == 0)
                {
                    var copy = new byte[index];
                    Buffer.BlockCopy(ret, 0, copy, 0, index);
                    return copy;
                }

                index += read;
            }

            return ret;
        }

        /// <summary>
        ///     Reads the specified number of bytes, returning them in a new byte array.
        ///     If not enough bytes are available before the end of the stream, this
        ///     method will throw an IOException.
        /// </summary>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>The bytes read</returns>
        public virtual byte[] ReadBytesOrThrow(int count)
        {
            var ret = new byte[count];
            readInternal(ret, count);
            return ret;
        }

        /// <summary>
        ///     Reads a 7-bit encoded integer from the stream. This is stored with the least significant
        ///     information first, with 7 bits of information per byte of value, and the top
        ///     bit as a continuation flag. This method is not affected by the endianness
        ///     of the bit converter.
        /// </summary>
        /// <returns>The 7-bit encoded integer read from the stream.</returns>
        public int Read7BitEncodedInt()
        {
            checkDisposed();

            int ret = 0;
            for (int shift = 0; shift < 35; shift += 7)
            {
                int b = BaseStream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();

                ret = ret | ((b & 0x7f) << shift);
                if ((b & 0x80) == 0)
                    return ret;
            }
            // Still haven't seen a byte with the high bit unset? Dodgy data.
            throw new IOException("Invalid 7-bit encoded integer in stream.");
        }

        /// <summary>
        ///     Reads a 7-bit encoded integer from the stream. This is stored with the most significant
        ///     information first, with 7 bits of information per byte of value, and the top
        ///     bit as a continuation flag. This method is not affected by the endianness
        ///     of the bit converter.
        /// </summary>
        /// <returns>The 7-bit encoded integer read from the stream.</returns>
        public int ReadBigEndian7BitEncodedInt()
        {
            checkDisposed();

            int ret = 0;
            for (int i = 0; i < 5; i++)
            {
                int b = BaseStream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();

                ret = (ret << 7) | (b & 0x7f);
                if ((b & 0x80) == 0)
                    return ret;
            }
            // Still haven't seen a byte with the high bit unset? Dodgy data.
            throw new IOException("Invalid 7-bit encoded integer in stream.");
        }

        /// <summary>
        ///     Reads a length-prefixed string from the stream, using the encoding for this reader.
        ///     A 7-bit encoded integer is first read, which specifies the number of bytes
        ///     to read from the stream. These bytes are then converted into a string with
        ///     the encoding for this reader.
        /// </summary>
        /// <returns>The string read from the stream.</returns>
        public virtual string ReadString()
        {
            int bytesToRead = Read7BitEncodedInt();

            var data = new byte[bytesToRead];
            readInternal(data, bytesToRead);
            return Encoding.GetString(data, 0, data.Length);
        }


        /// <summary>
        ///     Checks whether or not the reader has been _disposed, throwing an exception if so.
        /// </summary>
        private void checkDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("EndianBinaryReader");
        }

        /// <summary>
        ///     Reads the given number of bytes from the stream, throwing an exception
        ///     if they can't all be read.
        /// </summary>
        /// <param name="data">Buffer to read into</param>
        /// <param name="size">Number of bytes to read</param>
        private void readInternal(byte[] data, int size)
        {
            checkDisposed();
            int index = 0;
            while (index < size)
            {
                int read = BaseStream.Read(data, index, size - index);
                if (read == 0)
                    throw new EndOfStreamException(
                        $"End of stream reached with {size - index} byte{(size - index == 1 ? "s" : "")} left to read.");

                index += read;
            }
        }

        /// <summary>
        ///     Reads the given number of bytes from the stream if possible, returning
        ///     the number of bytes actually read, which may be less than requested if
        ///     (and only if) the end of the stream is reached.
        /// </summary>
        /// <param name="data">Buffer to read into</param>
        /// <param name="size">Number of bytes to read</param>
        /// <returns>Number of bytes actually read</returns>
        private int tryReadInternal(byte[] data, int size)
        {
            checkDisposed();
            int index = 0;
            while (index < size)
            {
                int read = BaseStream.Read(data, index, size - index);
                if (read == 0)
                    return index;

                index += read;
            }

            return index;
        }
    }
}