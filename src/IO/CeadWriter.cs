﻿using CeadLibrary.Extensions;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace CeadLibrary.IO
{
    public class CeadWriter : IDisposable
    {
        private const int MaxArrayPoolRentalSize = 64 * 1024;

        protected Stream _stream;
        private readonly Encoding _encoding;
        private readonly bool _leaveOpen;

        public CeadWriter(Stream output) : this(output, Encoding.UTF8, false) { }
        public CeadWriter(Stream output, Encoding encoding) : this(output, encoding, false) { }
        public CeadWriter(Stream output, Encoding encoding, bool leaveOpen)
        {
            _stream = output;
            _encoding = encoding;
            _leaveOpen = leaveOpen;

            Endian = BitConverter.IsLittleEndian ? Endian.Little : Endian.Big;
        }

        public void Dispose() => Dispose(true);
        public virtual void Close() => Dispose(true);
        public virtual void Dispose(bool disposing)
        {
            if (disposing) {
                if (_leaveOpen) {
                    _stream.Flush();
                }
                else {
                    _stream.Close();
                }
            }
        }

        public List<(long Offset, int Size)> Pointers { get; set; }
        public Endian Endian { get; set; }
        public virtual Stream BaseStream {
            get {
                _stream.Flush();
                return _stream;
            }
        }

        public virtual void Flush() => _stream.Flush();
        public virtual long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
        public virtual void Align(long alignment)
        {
            _stream.Seek((alignment - _stream.Position % alignment) % alignment, SeekOrigin.Current);
        }

        public virtual void Write(Span<byte> buffer) => _stream.Write(buffer);
        public virtual void Write(byte[] buffer) => _stream.Write(buffer, 0, buffer.Length);
        public virtual void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public void Write(decimal value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            DecimalExtension.GetBytes(value, buffer);
            _stream.Write(buffer);
        }

        public void Write(double value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(short value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteInt16BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(ushort value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(uint value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(long value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteInt64BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(ulong value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(Half value)
        {
            Span<byte> buffer = stackalloc byte[2];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteHalfBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteHalfLittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(float value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteSingleBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            }

            _stream.Write(buffer);
        }

        public void Write(byte value)
        {
            _stream.WriteByte(value);
        }

        public void Write(sbyte value)
        {
            _stream.WriteByte((byte)value);
        }

        public void Write(bool value, BoolType type = BoolType.Byte)
        {
            if (value) {
                Span<byte> buffer = stackalloc byte[(int)type];
                buffer[Endian == Endian.Big ? (^1) : 0] = 0x01;
                _stream.Write(buffer);
            }
            else {
                _stream.Seek((int)type, SeekOrigin.Current);
            }
        }

        public void Write(ReadOnlySpan<char> value, StringType? type = null)
        {
            if (value.Length <= MaxArrayPoolRentalSize / 3) {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(value.Length * 3);
                int actualByteCount = _encoding.GetBytes(value, buffer);
                WritePrefix(actualByteCount);

                return;
            }

            byte[] rented;
            int byteCount = _encoding.GetByteCount(value);
            WritePrefix(byteCount);

            if (byteCount <= MaxArrayPoolRentalSize) {
                rented = ArrayPool<byte>.Shared.Rent(byteCount);
                _encoding.GetBytes(value, rented);
                Write(rented, 0, byteCount);
                ArrayPool<byte>.Shared.Return(rented);
                return;
            }

            rented = ArrayPool<byte>.Shared.Rent(MaxArrayPoolRentalSize);
            Encoder encoder = _encoding.GetEncoder();
            bool completed;

            do {
                encoder.Convert(value, rented, flush: true, out int charsConsumed, out int bytesWritten, out completed);
                if (bytesWritten != 0) {
                    Write(rented, 0, bytesWritten);
                }

                value = value[..charsConsumed];
            } while (!completed);

            ArrayPool<byte>.Shared.Return(rented);

            void WritePrefix(int actualByteCount)
            {
                if (type == StringType.Int16CharCount) {
                    Write((ushort)actualByteCount);
                }
                else if (type == StringType.Int32CharCount) {
                    Write((ushort)actualByteCount);
                }
            }
        }

        public Action WriteObjectPtr(Action writeObject) => WriteObjectPtr<long>(writeObject);
        public Action WriteObjectPtr<PtrType>(Action writeObject) where PtrType : struct
        {
            long offset = _stream.Position;

            Action writePtr = typeof(PtrType).Name switch {
                "Byte" => () => WritePtr((pos) => Write((byte)pos), byte.MaxValue),
                "SByte" => () => WritePtr((pos) => Write((sbyte)pos), (ulong)sbyte.MaxValue),
                "Int16" => () => WritePtr((pos) => Write((short)pos), (ulong)short.MaxValue),
                "UInt16" => () => WritePtr((pos) => Write((ushort)pos), ushort.MaxValue),
                "Int32" => () => WritePtr((pos) => Write((int)pos), int.MaxValue),
                "UInt32" => () => WritePtr((pos) => Write((uint)pos), uint.MaxValue),
                "Int64" => () => WritePtr(Write, long.MaxValue),
                "UInt64" => () => WritePtr((pos) => Write((ulong)pos), ulong.MaxValue),
                _ => throw new InvalidTypeException(
                    typeof(PtrType), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong))
            };

            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<PtrType>()];
            _stream.Write(buffer);

            return writePtr;

            void WritePtr(Action<long> writePtr, ulong ptrTypeMaxSize)
            {
                long pos = _stream.Position;
                if ((ulong)pos > ptrTypeMaxSize) {
                    throw new OverflowException($"Could not pack the value '{offset}' into a {typeof(PtrType)} struct ({ptrTypeMaxSize})");
                }

                _stream.Seek(offset, SeekOrigin.Begin);
                writePtr(pos);
                _stream.Seek(pos, SeekOrigin.Begin);
                writeObject();
            }
        }
    }
}