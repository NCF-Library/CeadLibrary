#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using BenchmarkDotNet.Attributes;

namespace Tests
{
    [MemoryDiagnoser(false)]
    public class AlignBenchmark
    {
        private const long alignment = 1024;

        private readonly string lorem1 = "D:\\Bin\\BinaryLorem1.bin";
        private readonly string lorem2 = "D:\\Bin\\BinaryLorem2.bin";
        private Stream stream1;
        private Stream stream2;

        [GlobalSetup]
        public void Setup()
        {
            File.Copy(lorem1, lorem1 + ".restore");
            File.Copy(lorem2, lorem2 + ".restore");

            stream1 = File.OpenWrite(lorem1);
            stream1.Position = stream1.Length;

            stream2 = File.OpenWrite(lorem2);
            stream2.Position = stream2.Length;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            stream1.WriteByte(1);
            stream1.Flush();
            stream1.Dispose();

            stream2.WriteByte(1);
            stream2.Flush();
            stream2.Dispose();

            File.Move(lorem1, lorem1 + ".review", true);
            File.Move(lorem2, lorem2 + ".review", true);

            File.Move(lorem1 + ".restore", lorem1);
            File.Move(lorem2 + ".restore", lorem2);
        }

        [Benchmark] // 19.77 ns | 0 B (seek [?])
        public long AlignWithSeek()
        {
            stream1.Seek((alignment - stream1.Position % alignment) % alignment, SeekOrigin.Current);
            return stream1.Position;
        }

        [Benchmark] // 24.99 ns | 24 B (seek [?])
        public long AlignWithBuffer()
        {
            // For some reason I benchmarked this
            // as faster in SarcLibrary, not really
            // sure why though (it's not, btw)
            byte[] buffer = new byte[(alignment - stream2.Position % alignment) % alignment];
            stream2.Write(buffer, 0, buffer.Length);
            return stream2.Position;
        }
    }
}
