#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

using CeadLibrary.Extensions;
using CeadLibrary.Generics;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;

namespace CeadLibrary.IO
{
    [DebuggerDisplay("Position = {_stream.Position}, Length = {_stream.Length}, Endian = {Endian}, Encoding = {_encoding.WebName.ToUpper()}")]
    public class CeadReader : IDisposable
    {
        private readonly Stream _stream;
        public Encoding _encoding;
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
            _charSize = encoding is UnicodeEncoding ? 2 : 1;
            _leaveOpen = leaveOpen;

            Endian = BitConverter.IsLittleEndian ? Endian.Little : Endian.Big;
        }

        public Endian Endian { get; set; }
        public virtual Stream BaseStream => _stream;

        public virtual void Flush() => _stream.Flush();
        public virtual void Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
        public virtual void Align(long alignment)
        {
            _stream.Seek((alignment - _stream.Position % alignment) % alignment, SeekOrigin.Current);
        }

        public virtual int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public virtual int Read(Span<byte> buffer)
        {
            int numRead = _stream.Read(buffer);
            if (numRead > buffer.Length) {
                throw new EndOfStreamException($"Could not read {sizeof(decimal)} bytes");
            }

            return numRead;
        }

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

        public virtual decimal ReadDecimal()
        {
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            Read(buffer);

            return DecimalExtension.ToDecimal(buffer);
        }

        public virtual double ReadDouble()
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            }
        }

        public virtual short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadInt16LittleEndian(buffer);
            }
        }

        public virtual ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
            }
        }

        public virtual int ReadInt24()
        {
            Span<byte> buffer = stackalloc byte[4];

            if (Endian == Endian.Big) {
                Read(buffer[1..]);
                return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            else {
                Read(buffer[..^1]);
                return BinaryPrimitives.ReadInt32LittleEndian(buffer);
            }
        }

        public virtual uint ReadUInt24()
        {
            Span<byte> buffer = stackalloc byte[4];

            if (Endian == Endian.Big) {
                Read(buffer[1..]);
                return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            else {
                Read(buffer[..^1]);
                return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            }
        }

        public virtual int ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadInt32LittleEndian(buffer);
            }
        }

        public virtual uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            }
        }

        public virtual long ReadInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadInt64LittleEndian(buffer);
            }
        }

        public virtual ulong ReadUInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            }
        }

        public virtual Half ReadHalf()
        {
            Span<byte> buffer = stackalloc byte[2];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadHalfBigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadHalfLittleEndian(buffer);
            }
        }

        public virtual float ReadSingle()
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            Read(buffer);

            if (Endian == Endian.Big) {
                return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
            else {
                return BinaryPrimitives.ReadSingleLittleEndian(buffer);
            }
        }

        public virtual byte ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            Read(buffer);

            return buffer[0];
        }

        public virtual sbyte ReadSByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            Read(buffer);

            return (sbyte)buffer[0];
        }

        public virtual bool ReadBool(BoolType type, bool exactMatch = false)
        {
            Span<byte> buffer = stackalloc byte[(int)type];
            Read(buffer);

            if (exactMatch) {
                return buffer.SequenceEqual(Endian == Endian.Big ? "\0\0\0\x01"u8 : "\x01\0\0\0"u8);
            }
            
            for (int i = 0; i < (int)type; i++) {
                if (buffer[i] != 0) {
                    return true;
                }
            }

            return false;
        }

        public virtual string ReadString(int length)
        {
            Span<byte> buffer = new byte[length];
            Read(buffer);
            return _encoding.GetString(buffer);
        }

        public virtual string ReadString(StringType type)
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
                Seek(-(length + 1), SeekOrigin.Current);
                Read(data);
                return _encoding.GetString(data);
            }
            else if (type == StringType.Int16CharCount || type == StringType.Int32CharCount) {
                int length = type == StringType.Int32CharCount ? ReadInt32() : ReadInt16();
                Span<byte> buffer = new byte[length];
                Read(buffer);
                return _encoding.GetString(buffer);
            }

            throw new InvalidTypeException(typeof(StringType), type);
        }

        public virtual TEnum ReadEnum<TEnum>() where TEnum : Enum
        {
            string valueType = typeof(TEnum).GetEnumUnderlyingType().Name;
            object obj = valueType switch {
                "Byte" => ReadByte(),
                "SByte" => ReadSByte(),
                "Int16" => ReadInt16(),
                "UInt16" => ReadUInt16(),
                "Int32" => ReadInt32(),
                "UInt32" => ReadUInt32(),
                "Int64" => ReadInt64(),
                "UInt64" => ReadUInt64(),
                _ => throw new InvalidTypeException($"Invalid enum underlying type '{valueType}'")
            };

            return (TEnum)obj;
        }

        public virtual Endian ReadByteOrderMark()
        {
            ushort bom = ReadUInt16();
            Endian = Endian == Endian.Little ? (Endian)bom : bom == 0xFEFF ? Endian.Big : Endian.Little;
            return Endian;
        }

        public virtual SeekContext TemporarySeek(long offset, SeekOrigin origin)
        {
            return new(_stream, offset, origin);
        }

        public virtual T ReadObjectPtr<T>(SeekOrigin origin = SeekOrigin.Begin) where T : ICeadObject, new() => ReadObject<T>(ReadInt64(), origin);
        public virtual T ReadObjectPtr<T>(Func<T> read, SeekOrigin origin = SeekOrigin.Begin) => ReadObject(ReadInt64(), origin, read);
        public virtual T ReadObject<T>() where T : ICeadObject, new() => (T)new T().Read(this);
        public virtual T ReadObject<T>(long offset, SeekOrigin origin) where T : ICeadObject, new() => ReadObject(offset, origin, () => (T)new T().Read(this));
        public virtual T ReadObject<T>(long offset, SeekOrigin origin, Func<T> read)
        {
            T results;
            long origPos = _stream.Position;

            // Avoid redundant seeking 
            if (offset != 0 || origin != SeekOrigin.Current) {
                Seek(offset, origin);
            }

            results = read();
            Seek(origPos, SeekOrigin.Begin);
            return results;
        }

        public virtual T[] ReadObjectsPtr<T>(int count, SeekOrigin origin = SeekOrigin.Begin) where T : ICeadObject, new() => ReadObjects<T>(count, ReadInt64(), origin);
        public virtual T[] ReadObjectsPtr<T>(int count, Func<T> readObject, SeekOrigin origin = SeekOrigin.Begin) => ReadObjects(count, ReadInt64(), origin, readObject);
        public virtual T[] ReadObjects<T>(int count) where T : ICeadObject, new() => ReadObjects(count, 0, SeekOrigin.Current, () => (T)new T().Read(this));
        public virtual T[] ReadObjects<T>(int count, Func<T> readObject) => ReadObjects(count, 0, SeekOrigin.Current, readObject);
        public virtual T[] ReadObjects<T>(int count, long offset, SeekOrigin origin) where T : ICeadObject, new() => ReadObjects(count, offset, origin, () => (T)new T().Read(this));
        public virtual T[] ReadObjects<T>(int count, long offset, SeekOrigin origin, Func<T> readObject)
        {
            T[] objects = new T[count];

            return ReadObject(offset, origin, () => {
                for (int i = 0; i < count; i++) {
                    objects[i] = readObject();
                }

                return objects;
            });
        }

        public virtual bool CheckMagic(ReadOnlySpan<byte> expectedMagic, bool throwException = true)
        {
            // Only allocate if the request size
            // is more than 256 (which would be unusual)
            Span<byte> receivedMagic = expectedMagic.Length > 256 ? new byte[expectedMagic.Length] : stackalloc byte[expectedMagic.Length];
            Read(receivedMagic);

            return receivedMagic.SequenceEqual(expectedMagic) || (throwException ? throw new InvalidMagicException(expectedMagic, receivedMagic) : false);
        }
    }
}
