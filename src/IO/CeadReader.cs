using CeadLibrary.Extensions;
using CeadLibrary.Generics;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CeadLibrary.IO
{
    [DebuggerDisplay("Position = {_stream.Position}, Length = {_stream.Length}, Endian = {Endian}, Endoding = {_encoding.WebName.ToUpper()}")]
    public class CeadReader : IDisposable
    {
        private readonly Stream _stream;
        public Encoding _encoding;
        private readonly byte[] _buffer;
        private readonly int _charSize;
        private readonly bool _leaveOpen;
        private bool _disposed;

        public CeadReader(Stream input) : this(input, Encoding.UTF8, false) { }
        public CeadReader(Stream input, Encoding encoding) : this(input, encoding, false) { }
        public CeadReader(Stream input, Encoding encoding, bool leaveOpen)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(encoding);

            if (!input.CanRead) {
                throw new Exception("Stream was not readable");
            }

            _stream = input;
            _encoding = encoding;

            int minBufferSize = encoding.GetMaxByteCount(1);
            if (minBufferSize < 16) {
                minBufferSize = 16;
            }

            _buffer = new byte[minBufferSize];
            _charSize = encoding is UnicodeEncoding ? 2 : 1;
            _leaveOpen = leaveOpen;

            Endian = BitConverter.IsLittleEndian ? Endian.Little : Endian.Big;
        }

        public virtual Stream BaseStream => _stream;

        public virtual void Flush() => _stream.Flush();
        public virtual void Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
        public virtual void Align(long alignment)
        {
            _stream.Seek((alignment - _stream.Position % alignment) % alignment, SeekOrigin.Current);
        }

        public virtual int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public virtual int Read(Span<byte> buffer) => _stream.Read(buffer);

        public void Dispose() => Dispose(true);
        public virtual void Dispose(bool disposing)
        {
            if (!_disposed) {
                if (disposing && !_leaveOpen) {
                    _stream.Close();
                }
                _disposed = true;
            }
        }

        public virtual byte[] ReadBytes(int count)
        {
            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number required");
            }

            if (count == 0) {
                return Array.Empty<byte>();
            }

            byte[] buffer = new byte[count];
            _stream.ReadExactly(buffer);
            return buffer;
        }

        public Endian Endian { get; set; }

        public decimal ReadDecimal()
        {
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            if (_stream.Read(buffer) < sizeof(decimal)) {
                throw new EndOfStreamException($"Could not read {sizeof(decimal)} bytes.");
            }

            return DecimalExtension.ToDecimal(buffer);
        }

        public double ReadDouble()
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            if (_stream.Read(buffer) < sizeof(double)) {
                throw new EndOfStreamException($"Could not read {sizeof(double)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            }
        }

        public short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            if (_stream.Read(buffer) < sizeof(short)) {
                throw new EndOfStreamException($"Could not read {sizeof(short)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadInt16LittleEndian(buffer);
            }
        }

        public ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            if (_stream.Read(buffer) < sizeof(ushort)) {
                throw new EndOfStreamException($"Could not read {sizeof(ushort)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
            }
        }

        public int ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            if (_stream.Read(buffer) < sizeof(int)) {
                throw new EndOfStreamException($"Could not read {sizeof(int)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadInt32LittleEndian(buffer);
            }
        }

        public uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            if (_stream.Read(buffer) < sizeof(uint)) {
                throw new EndOfStreamException($"Could not read {sizeof(uint)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            }
        }

        public long ReadInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            if (_stream.Read(buffer) < sizeof(long)) {
                throw new EndOfStreamException($"Could not read {sizeof(long)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadInt64LittleEndian(buffer);
            }
        }

        public ulong ReadUInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            if (_stream.Read(buffer) < sizeof(ulong)) {
                throw new EndOfStreamException($"Could not read {sizeof(ulong)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            }
        }

        public Half ReadHalf()
        {
            Span<byte> buffer = stackalloc byte[2];
            if (_stream.Read(buffer) < 2) {
                throw new EndOfStreamException($"Could not read {2} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadHalfBigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadHalfLittleEndian(buffer);
            }
        }

        public float ReadSingle()
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            if (_stream.Read(buffer) < sizeof(float)) {
                throw new EndOfStreamException($"Could not read {sizeof(float)} bytes.");
            }

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadSingleLittleEndian(buffer);
            }
        }

        public bool ReadBool(BoolType type)
        {
            Span<byte> buffer = stackalloc byte[(int)type];
            if (_stream.Read(buffer) < (int)type) {
                throw new EndOfStreamException($"Could not read {(int)type} bytes.");
            }

            for (int i = 0; i < (int)type; i++) {
                if (buffer[i] != 0) {
                    return true;
                }
            }

            return false;
        }

        public byte ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            if (_stream.Read(buffer) < 1) {
                throw new EndOfStreamException($"Could not read {1} bytes.");
            }

            return buffer[0];
        }

        public sbyte ReadSByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            if (_stream.Read(buffer) < 1) {
                throw new EndOfStreamException($"Could not read {1} bytes.");
            }

            return (sbyte)buffer[0];
        }

        public string ReadString(int length)
        {
            Span<byte> buffer = new byte[length];
            _stream.Read(buffer);
            return _encoding.GetString(buffer);
        }

        public string ReadString(StringType type)
        {
            if (type == StringType.ZeroTerminated) {
                int length = 0;
                bool isChar = true;
                Span<byte> buffer = stackalloc byte[_charSize];
                while (isChar) {
                    int read = _stream.Read(buffer);
                    if (isChar = read > 0 && buffer[0] != 0 && (_charSize < 2 || buffer[1] != 0)) {
                        length += _charSize;
                    }
                }

                Span<byte> data = new byte[length];
                _stream.Seek(-(length + 1), SeekOrigin.Current);
                _stream.Read(data);
                return _encoding.GetString(data);
            }
            else if (type == (StringType.Int16CharCount | StringType.Int32CharCount)) {
                int length = (int)type == 2 ? ReadInt16() : ReadInt32();
                Span<byte> buffer = new byte[length];
                _stream.Read(buffer);
                return _encoding.GetString(buffer);
            }
            else {
                throw new InvalidTypeException(typeof(StringType), type);
            }
        }

        public T ReadObjectPtr<T>(SeekOrigin origin = SeekOrigin.Begin) where T : ICeadObject, new() => ReadObject<T>(ReadInt64(), origin);
        public T ReadObject<T>(long offset, SeekOrigin origin) where T : ICeadObject, new() => ReadObject(offset, origin, () => (T)new T().Read(this));
        public T ReadObject<T>(long offset, SeekOrigin origin, Func<T> read)
        {
            T results;

            // Avoid redundant seeking 
            if (offset == 0 && origin == SeekOrigin.Current) {
                results = read();
            }
            else {
                long origPos = _stream.Position;
                _stream.Seek(offset, origin);
                results = read();
                _stream.Seek(origPos, origin);
            }

            return results;
        }

        public T[] ReadObjectsPtr<T>(int count, SeekOrigin origin = SeekOrigin.Begin) where T : ICeadObject, new() => ReadObjects<T>(count, ReadInt64(), origin);
        public T[] ReadObjects<T>(int count, long offset, SeekOrigin origin) where T : ICeadObject, new() => ReadObjects(count, offset, origin, () => (T)new T().Read(this));
        public T[] ReadObjects<T>(int count, long offset, SeekOrigin origin, Func<T> readObject) where T : ICeadObject, new()
        {
            T[] objects = new T[count];

            return ReadObject(offset, origin, () => {
                for (int i = 0; i < count; i++) {
                    objects[i] = readObject();
                }

                return objects;
            });
        }
    }
}
