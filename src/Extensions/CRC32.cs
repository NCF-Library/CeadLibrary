namespace CeadLibrary.Extensions
{
    public static class CRC32
    {
        /// <summary>
        /// Compute the checksum of a span of characters
        /// </summary>
        public static uint Compute(ReadOnlySpan<char> data)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < data.Length; i++) {
                crc ^= (byte)data[i];
                for (int j = 0; j < 8; j++) {
                    uint mask = (uint)-(crc & 1);
                    crc = (crc >> 1) ^ (0xEDB88320 & mask);
                }
            }

            return ~crc;
        }

        /// <summary>
        /// Compute the checksum of a binary buffer
        /// </summary>
        public static uint Compute(ReadOnlySpan<byte> data)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < data.Length; i++) {
                crc ^= data[i];
                for (int j = 0; j < 8; j++) {
                    uint mask = (uint)-(crc & 1);
                    crc = (crc >> 1) ^ (0xEDB88320 & mask);
                }
            }

            return ~crc;
        }
    }
}
