using System.Buffers.Binary;

namespace NCF.BinaryExtensions
{
    public static class ReaderExtension
    {
        public static decimal ReadDecimal(this BinaryReader reader)
        {
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            if (reader.BaseStream.Read(buffer) < sizeof(decimal)) {
                throw new EndOfStreamException($"Could not read {sizeof(decimal)} bytes.");
            }

            return DataConverter.ToDecimal(buffer);
        }

        public static double ReadDouble(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            if (reader.BaseStream.Read(buffer) < sizeof(double)) {
                throw new EndOfStreamException($"Could not read {sizeof(double)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadDoubleBigEndian(buffer) : BinaryPrimitives.ReadDoubleLittleEndian(buffer);
        }

        public static double ReadInt16(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            if (reader.BaseStream.Read(buffer) < sizeof(short)) {
                throw new EndOfStreamException($"Could not read {sizeof(short)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadInt16BigEndian(buffer) : BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        public static ushort ReadUInt16(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            if (reader.BaseStream.Read(buffer) < sizeof(ushort)) {
                throw new EndOfStreamException($"Could not read {sizeof(ushort)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadUInt16BigEndian(buffer) : BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        public static double ReadInt32(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            if (reader.BaseStream.Read(buffer) < sizeof(int)) {
                throw new EndOfStreamException($"Could not read {sizeof(int)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadInt32BigEndian(buffer) : BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        public static uint ReadUInt32(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            if (reader.BaseStream.Read(buffer) < sizeof(uint)) {
                throw new EndOfStreamException($"Could not read {sizeof(uint)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadUInt32BigEndian(buffer) : BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        public static long ReadInt64(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            if (reader.BaseStream.Read(buffer) < sizeof(long)) {
                throw new EndOfStreamException($"Could not read {sizeof(long)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadInt64BigEndian(buffer) : BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        public static ulong ReadUInt64(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            if (reader.BaseStream.Read(buffer) < sizeof(ulong)) {
                throw new EndOfStreamException($"Could not read {sizeof(ulong)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadUInt64BigEndian(buffer) : BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        public static Half ReadHalf(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[2];
            if (reader.BaseStream.Read(buffer) < 2) {
                throw new EndOfStreamException($"Could not read {2} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadHalfBigEndian(buffer) : BinaryPrimitives.ReadHalfLittleEndian(buffer);
        }

        public static float ReadSingle(this BinaryReader reader, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            if (reader.BaseStream.Read(buffer) < sizeof(float)) {
                throw new EndOfStreamException($"Could not read {sizeof(float)} bytes.");
            }

            return endian == Endian.Big ? BinaryPrimitives.ReadSingleBigEndian(buffer) : BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        public static bool ReadBool(this BinaryReader reader, BoolType type, Endian? endian = null)
        {
            Span<byte> buffer = stackalloc byte[(int)type];
            if (reader.BaseStream.Read(buffer) < (int)type) {
                throw new EndOfStreamException($"Could not read {(int)type} bytes.");
            }

            if (endian == null) {
                for (int i = 0; i < (int)type; i++) {
                    if (buffer[i] != 0) {
                        return true;
                    }
                }
            }
            else {
                return buffer[endian == Endian.Big ? (^1) : 0] == 0x01;
            }

            return false;
        }
    }
}
