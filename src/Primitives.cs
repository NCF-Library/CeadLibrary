namespace NCF.BinaryExtensions
{
    public enum Endian : ushort
    {
        Big = 0xFFFE, Little = 0xFEFF
    }

    public enum BoolType : int
    {
        Byte = 1, Word = 2, DWord = 4, QWord = 8
    }

    public enum StringType
    {
        Int16CharCount, Int32CharCount, ZeroTerminated, Raw
    }

    public class InvalidBufferException : Exception
    {
        public InvalidBufferException() { }
        public InvalidBufferException(int bufferSize, int dataSize) : this($"The buffer size was smaller than the data size: {bufferSize} < {dataSize}") { }
        public InvalidBufferException(string? message) : base(message) { }
        public InvalidBufferException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
