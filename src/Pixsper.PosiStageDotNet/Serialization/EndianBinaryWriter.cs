﻿// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Pixsper.PosiStageDotNet.Serialization;
// Note: Based on code from MiscUtil r285 February 26th 2009 - http://www.yoda.arachsys.com/csharp/miscutil/

/// <summary>
///     Equivalent of <see cref="BinaryWriter" />, but with either endianness, depending on
///     the EndianBitConverter it is constructed with.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
[SuppressMessage("ReSharper", "VirtualMemberNeverOverriden.Global")]
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
internal class EndianBinaryWriter : IDisposable
{
	/// <summary>
	///     Buffer used for temporary storage during conversion from primitives
	/// </summary>
	readonly byte[] _buffer = new byte[16];

	/// <summary>
	///     Buffer used for Write(char)
	/// </summary>
	readonly char[] _charBuffer = new char[1];

	/// <summary>
	///     Whether or not this writer has been disposed yet.
	/// </summary>
	bool _isDisposed;



	/// <summary>
	///     Constructs a new binary writer with the given bit converter, writing
	///     to the given stream, using UTF-8 encoding.
	/// </summary>
	/// <param name="bitConverter">Converter to use when writing data</param>
	/// <param name="stream">Stream to write data to</param>
	public EndianBinaryWriter(EndianBitConverter bitConverter, Stream stream)
		: this(bitConverter, stream, Encoding.UTF8) { }

	/// <summary>
	///     Constructs a new binary writer with the given bit converter, writing
	///     to the given stream, using the given encoding.
	/// </summary>
	/// <param name="bitConverter">Converter to use when writing data</param>
	/// <param name="stream">Stream to write data to</param>
	/// <param name="encoding">Encoding to use when writing character data</param>
	/// <exception cref="ArgumentException">Stream isn't writable</exception>
	public EndianBinaryWriter(EndianBitConverter bitConverter, Stream stream, Encoding encoding)
	{
		if (!stream.CanWrite)
			throw new ArgumentException("Stream isn't writable", nameof(stream));

		BaseStream = stream;
		BitConverter = bitConverter;
		Encoding = encoding;
	}



	/// <summary>
	///     The bit converter used to write values to the stream
	/// </summary>
	public EndianBitConverter BitConverter { get; }

	/// <summary>
	///     The encoding used to write strings
	/// </summary>
	public Encoding Encoding { get; }

	/// <summary>
	///     Gets the underlying stream of the EndianBinaryWriter.
	/// </summary>
	public Stream BaseStream { get; }



	/// <summary>
	///     Disposes of the underlying stream.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		Flush();
		_isDisposed = true;
		((IDisposable)BaseStream).Dispose();
	}



	/// <summary>
	///     Closes the writer, including the underlying stream.
	/// </summary>
	public void Close()
	{
		Dispose();
	}

	/// <summary>
	///     Flushes the underlying stream.
	/// </summary>
	/// <exception cref="IOException">An I/O error occurs. </exception>
	public virtual void Flush()
	{
		checkDisposed();
		BaseStream.Flush();
	}

	/// <summary>
	///     Seeks within the stream.
	/// </summary>
	/// <param name="offset">Offset to seek to.</param>
	/// <param name="origin">Origin of seek operation.</param>
	/// <exception cref="NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
	/// <exception cref="IOException">An I/O error occurs. </exception>
	public virtual void Seek(long offset, SeekOrigin origin)
	{
		checkDisposed();
		BaseStream.Seek(offset, origin);
	}

	/// <summary>
	///     Writes a boolean value to the stream. 1 byte is written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(bool value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 1);
	}

	/// <summary>
	///     Writes a 16-bit signed integer to the stream, using the bit converter
	///     for this writer. 2 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(short value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 2);
	}

	/// <summary>
	///     Writes a 32-bit signed integer to the stream, using the bit converter
	///     for this writer. 4 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(int value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 4);
	}

	/// <summary>
	///     Writes a 64-bit signed integer to the stream, using the bit converter
	///     for this writer. 8 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(long value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 8);
	}

	/// <summary>
	///     Writes a 16-bit unsigned integer to the stream, using the bit converter
	///     for this writer. 2 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(ushort value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 2);
	}

	/// <summary>
	///     Writes a 32-bit unsigned integer to the stream, using the bit converter
	///     for this writer. 4 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(uint value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 4);
	}

	/// <summary>
	///     Writes a 64-bit unsigned integer to the stream, using the bit converter
	///     for this writer. 8 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(ulong value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 8);
	}

	/// <summary>
	///     Writes a single-precision floating-point value to the stream, using the bit converter
	///     for this writer. 4 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(float value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 4);
	}

	/// <summary>
	///     Writes a double-precision floating-point value to the stream, using the bit converter
	///     for this writer. 8 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(double value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 8);
	}

	/// <summary>
	///     Writes a decimal value to the stream, using the bit converter for this writer.
	///     16 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(decimal value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		writeInternal(_buffer, 16);
	}

	/// <summary>
	///     Writes a signed byte to the stream.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(byte value)
	{
		_buffer[0] = value;
		writeInternal(_buffer, 1);
	}

	/// <summary>
	///     Writes an unsigned byte to the stream.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(sbyte value)
	{
		_buffer[0] = unchecked((byte)value);
		writeInternal(_buffer, 1);
	}

	/// <summary>
	///     Writes an array of bytes to the stream.
	/// </summary>
	/// <param name="value">The values to write</param>
	public virtual void Write(byte[] value)
	{
		writeInternal(value, value.Length);
	}

	/// <summary>
	///     Writes a portion of an array of bytes to the stream.
	/// </summary>
	/// <param name="value">An array containing the bytes to write</param>
	/// <param name="offset">The index of the first byte to write within the array</param>
	/// <param name="count">The number of bytes to write</param>
	public virtual void Write(byte[] value, int offset, int count)
	{
		checkDisposed();
		BaseStream.Write(value, offset, count);
	}

	/// <summary>
	///     Writes a single character to the stream, using the encoding for this writer.
	/// </summary>
	/// <param name="value">The value to write</param>
	public virtual void Write(char value)
	{
		_charBuffer[0] = value;
		Write(_charBuffer);
	}

	/// <summary>
	///     Writes an array of characters to the stream, using the encoding for this writer.
	/// </summary>
	/// <param name="value">An array containing the characters to write</param>
	public virtual void Write(char[] value)
	{
		checkDisposed();
		var data = Encoding.GetBytes(value, 0, value.Length);
		writeInternal(data, data.Length);
	}

	/// <summary>
	///     Writes a string to the stream, using the encoding for this writer.
	/// </summary>
	/// <param name="value">The value to write. Must not be null.</param>
	/// <exception cref="ArgumentNullException">value is null</exception>
	public virtual void Write(string value)
	{
		checkDisposed();
		var data = Encoding.GetBytes(value);
		Write7BitEncodedInt(data.Length);
		writeInternal(data, data.Length);
	}

	/// <summary>
	///     Writes a 7-bit encoded integer from the stream. This is stored with the least significant
	///     information first, with 7 bits of information per byte of value, and the top
	///     bit as a continuation flag.
	/// </summary>
	/// <param name="value">The 7-bit encoded integer to write to the stream</param>
	public void Write7BitEncodedInt(int value)
	{
		checkDisposed();

		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to 0.");

		int index = 0;
		while (value >= 128)
		{
			_buffer[index++] = (byte)((value & 0x7f) | 0x80);
			value = value >> 7;
			++index;
		}
		_buffer[index++] = (byte)value;
		BaseStream.Write(_buffer, 0, index);
	}


	/// <summary>
	///     Checks whether or not the writer has been disposed, throwing an exception if so.
	/// </summary>
	void checkDisposed()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("EndianBinaryWriter");
		}
	}

	/// <summary>
	///     Writes the specified number of bytes from the start of the given byte array,
	///     after checking whether or not the writer has been disposed.
	/// </summary>
	/// <param name="bytes">The array of bytes to write from</param>
	/// <param name="length">The number of bytes to write</param>
	private void writeInternal(byte[] bytes, int length)
	{
		checkDisposed();
		BaseStream.Write(bytes, 0, length);
	}
}