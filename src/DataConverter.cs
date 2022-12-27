namespace NCF.BinaryExtensions
{
    public static class DataConverter
    {
        public static void GetBytes(decimal value, Span<byte> buffer)
        {
            if(buffer.Length < sizeof(decimal)) {
                throw new InvalidBufferException(buffer.Length, sizeof(decimal));
            }

            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < 4; i++) {
                int offset = i * sizeof(int);
                int bit = bits[i];
                buffer[offset] = (byte)bit;
                buffer[offset + 1] = (byte)(bit >> 8);
                buffer[offset + 2] = (byte)(bit >> 16);
                buffer[offset + 3] = (byte)(bit >> 24);
            }
        }

        public static decimal ToDecimal(Span<byte> data)
        {
            int[] bits = new int[4];
            for (int i = 0; i < 4; i++) {
                int offset = i * sizeof(int);
                bits[i] = data[offset]
                    | data[offset + 1] << 8
                    | data[offset + 2] << 16
                    | data[offset + 3] << 24;
            }

            return new(bits);
        }
    }
}
