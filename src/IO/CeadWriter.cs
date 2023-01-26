#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

using CeadLibrary.Extensions;
using CeadLibrary.Generics;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CeadLibrary.IO
{
    [DebuggerDisplay("Position = {_stream.Position}, Length = {_stream.Length}, Endian = {Endian}, Encoding = {_encoding.WebName.ToUpper()}")]
    public class CeadWriter : IDisposable
    {
        private const int MaxArrayPoolRentalSize = 64 * 1024;

        protected Stream _stream;
        protected readonly Encoding _encoding;
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

        public virtual void Write(byte[] buffer) => _stream.Write(buffer, 0, buffer.Length);
        public virtual void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
        public virtual void Write(ReadOnlySpan<byte> buffer)
        {
            if (GetType() == typeof(CeadWriter)) {
                _stream.Write(buffer);
            }
            else {
                byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);
                try {
                    buffer.CopyTo(array);
                    Write(array, 0, buffer.Length);
                }
                finally {
                    ArrayPool<byte>.Shared.Return(array);
                }
            }
        }

        public virtual void Write(decimal value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            DecimalExtension.GetBytes(value, buffer);
            Write(buffer);
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

            Write(buffer);
        }

        public virtual void Write(short value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteInt16BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void Write(ushort value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void WriteInt24(int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
                Write(buffer[1..]);
            }
            else {
                BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
                Write(buffer[..^1]);
            }
        }

        public virtual void WriteUInt24(uint value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
                Write(buffer[1..]);
            }
            else {
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
                Write(buffer[..^1]);
            }
        }

        public virtual void Write(int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void Write(uint value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void Write(long value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteInt64BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void Write(ulong value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void Write(Half value)
        {
            Span<byte> buffer = stackalloc byte[2];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteHalfBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteHalfLittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void Write(float value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            if (Endian == Endian.Big) {
                BinaryPrimitives.WriteSingleBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            }

            Write(buffer);
        }

        public virtual void Write(byte value)
        {
            _stream.WriteByte(value);
        }

        public virtual void Write(sbyte value)
        {
            _stream.WriteByte((byte)value);
        }

        public virtual void Write(bool value, BoolType type = BoolType.Byte)
        {
            if (value) {
                Span<byte> buffer = stackalloc byte[(int)type];
                buffer[Endian == Endian.Big ? (^1) : 0] = 0x01;
                Write(buffer);
            }
            else {
                Seek((int)type, SeekOrigin.Current);
            }
        }

        public virtual void Write(ReadOnlySpan<char> value, StringType? type = null)
        {
            if (value.Length <= MaxArrayPoolRentalSize / 3) {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(value.Length * 3);
                int actualByteCount = _encoding.GetBytes(value, buffer);
                WritePrefix(actualByteCount);
                Write(buffer, 0, actualByteCount);
                ArrayPool<byte>.Shared.Return(buffer);
                WritePostfix();
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
                WritePostfix();
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
            WritePostfix();

            void WritePrefix(int actualByteCount)
            {
                if (type == StringType.Int16CharCount) {
                    Write((ushort)actualByteCount);
                }
                else if (type == StringType.Int32CharCount) {
                    Write((ushort)actualByteCount);
                }
            }

            void WritePostfix()
            {
                if (type == StringType.ZeroTerminated) {
                    Write((byte)0);
                }
            }
        }

        public virtual void WritePascalString(ReadOnlySpan<char> value)
        {
            Write(value, StringType.Int16CharCount);
            Write((byte)0);
        }

        public virtual SeekContext TemporarySeek(long offset, SeekOrigin origin)
        {
            return new(_stream, offset, origin);
        }

        public virtual Action WriteObjectPtr(ICeadObject obj) => WriteObjectPtr<long>(() => obj.Write(this));
        public virtual Action WriteObjectPtr(Action writeObject) => WriteObjectPtr<long>(writeObject);
        public virtual Action WriteObjectPtr<PtrType>(ICeadObject obj) where PtrType : struct => WriteObjectPtr<PtrType>(() => obj.Write(this));
        public virtual Action WriteObjectPtr<PtrType>(Action writeObject) where PtrType : struct
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
            Write(buffer);

            return writePtr;

            void WritePtr(Action<long> writePtr, ulong ptrTypeMaxSize)
            {
                long pos = _stream.Position;
                if ((ulong)pos > ptrTypeMaxSize) {
                    throw new OverflowException($"Could not pack the value '{offset}' into a {typeof(PtrType)} struct ({ptrTypeMaxSize})");
                }

                Seek(offset, SeekOrigin.Begin);
                writePtr(pos);
                Seek(pos, SeekOrigin.Begin);
                writeObject();
            }
        }

        public virtual void WriteObjects<T>(IEnumerable<T> objects) where T : ICeadObject => WriteObjects(objects, (obj) => obj.Write(this));
        public virtual void WriteObjects<T>(IEnumerable<T> objects, Action<T> writeObject)
        {
            foreach (var obj in objects) {
                writeObject(obj);
            }
        }
    }
}
