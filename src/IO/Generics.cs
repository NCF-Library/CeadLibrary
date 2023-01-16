using System.Text.Json.Serialization;

namespace CeadLibrary.IO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Endian : ushort
    {
        Big = 0xFFFE, Little = 0xFEFF
    }

    public enum BoolType : int
    {
        Byte = 1, Word = 2, DWord = 4, QWord = 8
    }

    public enum StringType : int
    {
        ZeroTerminated = 0,
        Int16CharCount = 2,
        Int32CharCount = 4,
    }

    public class InvalidBufferException : Exception
    {
        public InvalidBufferException() { }
        public InvalidBufferException(int bufferSize, int dataSize) : this($"The buffer size was smaller than the data size: {bufferSize} < {dataSize}") { }
        public InvalidBufferException(string? message) : base(message) { }
        public InvalidBufferException(string? message, Exception? innerException) : base(message, innerException) { }
    }

    public class InvalidTypeException : Exception
    {
        public InvalidTypeException() { }
        public InvalidTypeException(Type validEnum, object receivedEnum) : base($"Could not find the enum value '{receivedEnum}' in the enum collection '{validEnum.Name}' ({string.Join(", ", validEnum.GetFields()[1..].Select(x => x.Name))})") { }
        public InvalidTypeException(Type receivedType, params Type[] validTypes) : base($"The type '{receivedType}' is not valid in this scope.\nValid types: '{string.Join(", ", validTypes.Select(x => x.Name))}'") { }
        public InvalidTypeException(string? message) : base(message) { }
        public InvalidTypeException(string? message, Exception? innerException) : base(message, innerException) { }
    }

    public class InvalidMagicException : Exception
    {
        public InvalidMagicException() { }
        public InvalidMagicException(ReadOnlySpan<byte> expectedMagic, ReadOnlySpan<byte> receivedMagic) : base($"The parser found '0x{Convert.ToHexString(receivedMagic)}' when it expected '0x{Convert.ToHexString(expectedMagic)}'") { }
        public InvalidMagicException(string? message) : base(message) { }
        public InvalidMagicException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
