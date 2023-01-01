using BenchmarkDotNet.Running;
using CeadLibrary.IO;
using System.Diagnostics;
using System.Numerics;
using Tests;
using Tests.Data;
using Tests.Extensions;

//
// Read String Benchmark

/// ReadStringBenchmark benchmark = new();
/// benchmark.Setup();
/// 
/// Console.WriteLine(benchmark.ReadStringEncoding().Length == 7522);
/// Console.WriteLine(benchmark.ReadStringDecoder().Length == 7522);
/// Console.WriteLine(benchmark.ReadNullTerminatedStringList().Length == 11);
/// Console.WriteLine(benchmark.ReadNullTerminatedStringCount().Length == 11);
/// Console.WriteLine(benchmark.ReadNullTerminatedStringDecoder().Length == 11);
/// 
/// BenchmarkRunner.Run<ReadStringBenchmark>();
/// 


//
// Align Benchmark

/// AlignBenchmark benchmark = new();
/// benchmark.Setup();
/// 
/// Console.WriteLine(benchmark.AlignWithSeek());
/// Console.WriteLine(benchmark.AlignWithBuffer());
/// 
/// benchmark.Cleanup();
/// 
/// BenchmarkRunner.Run<AlignBenchmark>();
///


//
// Generic Init Benchmark

/// GenericInitBenchmark benchmark = new();
/// 
/// Console.WriteLine(benchmark.PublicInit().Name);
/// Console.WriteLine(benchmark.GenericInit().Name);
/// 
/// BenchmarkRunner.Run<GenericInitBenchmark>();
///


//
// Writer Benchmark

/// WriterBenchmark benchmark = new();
/// 
/// benchmark.Setup();
/// 
/// benchmark.CeadWriter();
/// benchmark.BinaryWriter();
/// 
/// benchmark.Cleanup();
/// 
/// BenchmarkRunner.Run<WriterBenchmark>();
/// 