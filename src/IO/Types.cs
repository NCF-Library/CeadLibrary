namespace CeadLibrary.IO
{
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
        public InvalidTypeException(Type validTypes, object receivedType) : this($"Could not find the enum value '{receivedType}' in the enum collection '{validTypes.Name}' ({string.Join(", ", validTypes.GetFields()[1..].Select(x => x.Name))})") { }
        public InvalidTypeException(string? message) : base(message) { }
        public InvalidTypeException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
