using System;

namespace Gitan.SortedDictionary;

public class Program
{
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

