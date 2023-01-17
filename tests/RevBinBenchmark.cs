using BenchmarkDotNet.Attributes;
using System.Text;

namespace Tests
{
    [MemoryDiagnoser()]
    public class RevBinBenchmark
    {
        private readonly string str = "ABCDEFGH";
        private readonly Encoding _encoding = Encoding.UTF8;

        [Benchmark]
        public string RevOnString()
        {
            return string.Join("", string.Join("", _encoding.GetBytes(str).Select(n => Convert.ToString(n, 2).PadLeft(8, '0'))).Reverse());
        }

        [Benchmark]
        public Span<byte> RevOnSpan()
        {
            Span<byte> data = new byte[str.Length * 3];
            int size = _encoding.GetBytes(str, data);
            data = data[..size];

            for (int i = 0; i < size; i++) {
                data[i] = (byte)((data[i] * 0x0202020202ul & 0x010884422010ul) % 1023);
            }

            data.Reverse();
            return data;
        }
    }
}
