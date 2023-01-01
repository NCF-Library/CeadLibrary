using BenchmarkDotNet.Attributes;
using Tests.Data;

namespace Tests
{
    [MemoryDiagnoser(false)]
    public class GenericInitBenchmark
    {
        [Benchmark]
        public GenericObject PublicInit()
        {
            return new GenericObject();
        }

        [Benchmark]
        public GenericObject GenericInit()
        {
            return Activator.CreateInstance<GenericObject>();
        }
    }
}
