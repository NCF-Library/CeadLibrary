#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using BenchmarkDotNet.Attributes;
using CeadLibrary.IO;
using Tests.Data;
using Tests.Extensions;

namespace Tests
{
    [MemoryDiagnoser(false)]
    public class WriterBenchmark
    {
        private FileStream _bFs;
        private FileStream _cFs;
        private CeadWriter _ceadWriter;
        private BinaryWriter _binaryWriter;
        private List<GenericObject> _gobjs = new() {
            new("John", "Wick", 35),
            new("Marcus", "Smith", 41),
            new("Richard", "Stewart", 15),
            new("Mario", "", 64),
            new("Creed", "Skilar", 23),
        };

        [GlobalSetup]
        public void Setup()
        {
            _cFs = File.Create("C:\\cead.bin");
            _bFs = File.Create("C:\\binary.bin");
            _ceadWriter = new(_cFs);
            _binaryWriter = new(_bFs);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _cFs.Dispose();
            _ceadWriter.Dispose();
            File.Delete("C:\\cead.bin");

            _bFs.Dispose();
            _binaryWriter.Dispose();
            File.Delete("C:\\binary.bin");
        }

        [Benchmark]
        public void CeadWriter()
        {
            _ceadWriter.Seek(0, SeekOrigin.Begin);

            _ceadWriter.Write("GOBJLIST"u8);
            _ceadWriter.Write((ushort)_gobjs.Count);
            Action writeGobjList = _ceadWriter.WriteObjectPtr<ushort>(() => _ceadWriter.WriteObjects(_gobjs));
            _ceadWriter.Write((ushort)0xFEFF);
            _ceadWriter.Align(4);

            writeGobjList();
            _ceadWriter.Align(8);
        }

        [Benchmark]
        public void BinaryWriter()
        {
            _binaryWriter.Seek(0, SeekOrigin.Begin);

            _binaryWriter.Write("GOBJLIST"u8);
            _binaryWriter.Write((ushort)_gobjs.Count);
            Action writeGobjList = _binaryWriter.WriteObjectPtr<ushort>(() => _binaryWriter.WriteObjects(_gobjs));
            _binaryWriter.Write((ushort)0xFEFF);
            _binaryWriter.Align(4);

            writeGobjList();
            _binaryWriter.Align(8);
        }
    }
}
