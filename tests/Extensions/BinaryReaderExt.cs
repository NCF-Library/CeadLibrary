using CeadLibrary.IO;
using System.Text;

namespace Tests.Extensions
{
    public static class BinaryReaderExt
    {
        public static string ReadString(this BinaryReader reader, StringType type)
        {
            if (type == (StringType.Int16CharCount | StringType.Int32CharCount)) {
                int length = type == StringType.Int32CharCount ? reader.ReadInt32() : reader.ReadInt16();
                Span<byte> buffer = new byte[length];
                reader.Read(buffer);
                return Encoding.UTF8.GetString(buffer);
            }

            throw new InvalidTypeException(typeof(StringType), type);
        }

        public static void Align(this BinaryReader reader, long alignment)
        {
            reader.BaseStream.Seek((alignment - reader.BaseStream.Position % alignment) % alignment, SeekOrigin.Current);
        }

        public static bool CheckMagic(this BinaryReader reader, ReadOnlySpan<byte> expectedMagic, bool throwException = true)
        {
            // Only allocate if the request size
            // is more than 256 (which would be unusual)
            Span<byte> receivedMagic = expectedMagic.Length > 256 ? new byte[expectedMagic.Length] : stackalloc byte[expectedMagic.Length];
            reader.Read(receivedMagic);

            return receivedMagic.SequenceEqual(expectedMagic) || (throwException ? throw new InvalidMagicException(expectedMagic, receivedMagic) : false);
        }
    }
}
