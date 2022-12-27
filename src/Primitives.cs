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
}
