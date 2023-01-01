#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using BenchmarkDotNet.Attributes;
using System.Text;

namespace Tests
{
    [MemoryDiagnoser(false)]
    public class ReadStringBenchmark
    {
        private const int MaxCharBytesSize = 128;
        private const int MaxBuilderSize = 360;

        private Encoding encoding;
        private Decoder decoder;
        private Stream stream;
        private byte[]? _charBytes;
        private char[]? _charBuffer;
        private int count;
        private int maxCharsSize;  // From MaxCharBytesSize & Encoding

        [GlobalSetup]
        public void Setup()
        {
            count = 7522;
            encoding = Encoding.UTF8;
            decoder = encoding.GetDecoder();
            maxCharsSize = encoding.GetMaxByteCount(MaxCharBytesSize);
            stream = File.OpenRead("D:\\Bin\\BinaryLorem.bin");
        }

        [Benchmark] // 1.525 us | 22.09 KB (alloc 7522 chars)
        public string ReadStringEncoding()
        {
            stream.Seek(0, SeekOrigin.Begin);

            Span<byte> buffer = new byte[count];
            stream.Read(buffer);
            return encoding.GetString(buffer);
        }

        [Benchmark] // 4.424 us | 37.64 KB (alloc 7522 chars)
        public string ReadStringDecoder()
        {
            stream.Seek(0, SeekOrigin.Begin);

            int currPos = 0;
            int n;
            int readLength;
            int charsRead;

            if (count == 0) {
                return string.Empty;
            }

            _charBytes ??= new byte[MaxCharBytesSize];
            _charBuffer ??= new char[maxCharsSize];

            StringBuilder? sb = null;
            do {
                readLength = ((count - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (count - currPos);

                n = stream.Read(_charBytes, 0, readLength);
                if (n == 0) {
                    throw new Exception();
                }

                charsRead = decoder.GetChars(_charBytes, 0, n, _charBuffer, 0);

                if (currPos == 0 && n == count) {
                    return new string(_charBuffer, 0, charsRead);
                }

                // Since we could be reading from an untrusted data source, limit the initial size of the
                // StringBuilder instance we're about to get or create. It'll expand automatically as needed.

                sb ??= new StringBuilder(Math.Min(count, MaxBuilderSize)); // Actual string length in chars may be smaller.
                sb.Append(_charBuffer, 0, charsRead);
                currPos += n;
            } while (currPos < count);

            return sb.ToString();
        }

        [Benchmark] // 60.932 us | 38.41 KB (alloc 7522 chars, nullchar at 7523)
        public string ReadNullTerminatedStringList()
        {
            stream.Seek(0, SeekOrigin.Begin);

            List<byte> data = new();
            int charSize = encoding.GetByteCount("0");
            bool isChar = true;
            Span<byte> buffer = stackalloc byte[charSize];
            while (isChar) {
                int read = stream.Read(buffer);
                if (isChar = read > 0 && buffer[0] != 0 && (charSize < 2 || buffer[1] != 0)) {
                    data.Add(buffer[0]);

                    if (charSize > 1) {
                        data.Add(buffer[1]);
                    }
                }
            }

            return encoding.GetString(data.ToArray());
        }

        [Benchmark] // 51.507 us | 22.09 (alloc 7522 chars, nullchar at 7523)
        public string ReadNullTerminatedStringCount()
        {
            stream.Seek(0, SeekOrigin.Begin);

            int count = 0;
            int charSize = encoding.GetByteCount("0");
            bool isChar = true;
            Span<byte> buffer = stackalloc byte[charSize];
            while (isChar) {
                int read = stream.Read(buffer);
                if (isChar = read > 0 && buffer[0] != 0 && (charSize < 2 || buffer[1] != 0)) {
                    count += charSize;
                }
            }

            Span<byte> data = new byte[count];
            stream.Seek(-(count + 1), SeekOrigin.Current);
            stream.Read(data);
            return encoding.GetString(data);
        }

        [Benchmark] // 158.174 us | 31.42 (alloc 7522 chars, nullchar at 7523)
        public string ReadNullTerminatedStringDecoder()
        {
            stream.Seek(0, SeekOrigin.Begin);

            int charsRead;
            _charBytes ??= new byte[MaxCharBytesSize];
            _charBuffer ??= new char[maxCharsSize];

            StringBuilder sb = new();
            int charSize = encoding.GetByteCount("0");
            bool isChar = true;
            while (isChar) {
                int read = stream.Read(_charBytes, 0, charSize);
                if (isChar = read > 0 && _charBytes[0] != 0 && (charSize < 2 || _charBytes[1] != 0)) {
                    charsRead = decoder.GetChars(_charBytes, 0, read, _charBuffer, 0);
                    sb.Append(_charBuffer, 0, charsRead);
                }
            }

            return sb.ToString();
        }
    }
}
