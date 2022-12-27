using System.Buffers.Binary;

namespace NCF.BinaryExtensions
{
    public static class WriterExtension
    {
        public static void Write(this BinaryWriter writer, decimal value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            DataConverter.GetBytes(value, buffer);
            writer.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, double value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            if (endian == Endian.Big) {
                BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, short value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteInt16BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, ushort value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, int value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, uint value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, long value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteInt64BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, ulong value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, Half value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[2];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteHalfBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteHalfLittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, float value, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            if(endian == Endian.Big) {
                BinaryPrimitives.WriteSingleBigEndian(buffer, value);
            }
            else {
                BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            }

            writer.BaseStream.Write(buffer);
        }

        public static void Write(this BinaryWriter writer, bool value, BoolType type, Endian endian)
        {
            Span<byte> buffer = stackalloc byte[(int)type];
            buffer[endian == Endian.Big ? (^1) : 0] = 0x01;
            writer.BaseStream.Write(buffer);
        }
    }
}
