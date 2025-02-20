using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Benchmark;

internal class Program
{
    public static ReadOnlyCollection<ZipArchiveEntry> Entries;
    static void Main()
    {
        using ZipArchive zip = ZipFile.OpenRead("benchmark.zip");
        Entries = zip.Entries;
        if (RuntimeFeature.IsDynamicCodeCompiled && RuntimeFeature.IsDynamicCodeSupported && !IsDebug())
        {
            ManualConfig config = ManualConfig.CreateMinimumViable();
            config = config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
            BenchmarkRunner.Run<HelloWorld_NBT>(config);
            BenchmarkRunner.Run<Level_NBT>(config);
            BenchmarkRunner.Run<Level_1_NBT>(config);
            BenchmarkRunner.Run<Scoreboard_NBT>(config);
            BenchmarkRunner.Run<BigTest_NBT>(config);
            BenchmarkRunner.Run<BigTestASCII_NBT>(config);
            return;
        }
        Console.WriteLine("pls run in release mode");
        Scoreboard_NBT s = new();
        s.Benchmark_ElysiaNBT_ToObject();
        s.Benchmark_KonvesNbt_ToObject();
    }
    public static bool IsDebug()
    {
        Assembly assm = Assembly.GetExecutingAssembly();
        object[] attributes = assm.GetCustomAttributes(typeof(DebuggableAttribute), false);
        if (attributes.Length == 0)
            return false;
        foreach (object attr in attributes)
        {
            if (attr is not DebuggableAttribute d)
                continue;
            return d.IsJITOptimizerDisabled;
        }
        return false;
    }
}
