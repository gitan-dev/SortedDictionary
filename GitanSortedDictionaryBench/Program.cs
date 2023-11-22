using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Gitan.SortedDictionary;

public class Program
{
    //static void Main(string[] args)
    //{
    //    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    //}

    public static void Main(string[] args)
    {
#if DEBUG
            Console.WriteLine("Hello Debug");
#else
        Console.WriteLine("Hello Release");
#endif
        //BenchmarkDotNet.Running.BenchmarkRunner.Run<SortedDictionaryBench>();
        BenchmarkDotNet.Running.BenchmarkRunner.Run<SortedDictionaryBench2>();
    }
}

