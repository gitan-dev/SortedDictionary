using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
//using static Gitan.SortedDictionary.SortedDictionaryBench;

namespace Gitan.SortedDictionary;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]

public class SortedDictionaryBench
{
    //const int _loopCount = 10_0000;
    readonly object _lockObject = new();

    static readonly Random _random = new();

    public sealed class SortLong : System.Collections.Generic.IComparer<long>
    {
        public int Compare(long x, long y)
        {
            if (x < y) return -1;
            if (x == y) return 0;
            return 1;
        }
    }
    public sealed class ReverceSortLong : System.Collections.Generic.IComparer<long>
    {
        public int Compare(long x, long y)
        {
            if (x > y) return -1;
            if (x == y) return 0;
            return 1;
        }
    }

    public class OrderBoardDiff
    {
        public System.Collections.Generic.KeyValuePair<long, double>[] Bids { get; }
        public System.Collections.Generic.KeyValuePair<long, double>[] Asks { get; }

        public OrderBoardDiff(System.Collections.Generic.KeyValuePair<long, double>[] bids, System.Collections.Generic.KeyValuePair<long, double>[] asks)
        {
            Bids = bids;
            Asks = asks;
        }
    }

    //static readonly OrderBoardDiff[] _orderBoardDiffs = GetSourceData(10000, 20000, 18000, 28000, 100_000, 10);
    static readonly OrderBoardDiff[] _orderBoardDiffs = GetSourceData(1000, 2000, 1800, 2800, 100_000, 10);

    public static OrderBoardDiff[] GetSourceData(int bidMinPrice, int bidMaxPrice, int askMinPrice, int askMaxPrice, int diffSize, int sameSize)
    {
        var orderboardDiffList = new System.Collections.Generic.List<OrderBoardDiff>();

        for (int j = 0; j < diffSize; j++)
        {
            var bidsList = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<long, double>>();

            for (int i = 0; i < sameSize; i++)
            {
                var keyValuePair = new System.Collections.Generic.KeyValuePair<long, double>(_random.Next(bidMinPrice, bidMaxPrice), ((double)_random.Next(100)) / 1000 + 0.001);
                bidsList.Add(keyValuePair);
            }

            var asksList = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<long, double>>();

            for (int i = 0; i < sameSize; i++)
            {
                var keyValuePair = new System.Collections.Generic.KeyValuePair<long, double>(_random.Next(askMinPrice, askMaxPrice), ((double)_random.Next(100)) / 100 + 0.01);
                asksList.Add(keyValuePair);
            }

            var orderboardDiff = new OrderBoardDiff(bidsList.ToArray(), asksList.ToArray());
            orderboardDiffList.Add(orderboardDiff);
        }

        return orderboardDiffList.ToArray();
    }

    [Benchmark]
    public (System.Collections.Generic.SortedDictionary<long, double> bids, System.Collections.Generic.SortedDictionary<long, double> asks) SystemSortedDictionaryBench() => MakeOrderboard_SystemSortedDictionary(_orderBoardDiffs);


    public (System.Collections.Generic.SortedDictionary<long, double> bids, System.Collections.Generic.SortedDictionary<long, double> asks) MakeOrderboard_SystemSortedDictionary(OrderBoardDiff[] diffArray)
    {
        var bids = new System.Collections.Generic.SortedDictionary<long, double>(new ReverceSortLong());
        var asks = new System.Collections.Generic.SortedDictionary<long, double>(new SortLong());

        foreach (var diff in diffArray)
        {
            lock (_lockObject)
            {
                UpdateOrderBoard_SystemSortedDictionary(bids, asks, diff);
            }
            lock (_lockObject)
            {
#pragma warning disable IDE0042
                var result = GetPrice_SystemSortedDictionary(bids, asks);
#pragma warning restore IDE0042
            }
        }

        return (bids, asks);
    }

    public void UpdateOrderBoard_SystemSortedDictionary(System.Collections.Generic.SortedDictionary<long, double> bids, System.Collections.Generic.SortedDictionary<long, double> asks, OrderBoardDiff diff)
    {
        var bidsData = diff.Bids;
        for (int i = 0; i < bidsData.Length; i++)
        {
            var (price, size) = bidsData[i];
            if (size == 0.0)
            {
                bids.Remove(price); // あったら消す、無くても問題ない(なにもしない)
            }
            else
            {
                bids[price] = size;
            }
        }
        if (bids.Any())
        {
            var bidFirstPrice = bids.First().Key;
            while (true)
            {
                if (asks.Count == 0) { break; }
                var askFirstPrice = asks.First().Key;
                if (bidFirstPrice < askFirstPrice) { break; }
                asks.Remove(askFirstPrice);
            }
        }

        var asksData = diff.Asks;
        for (int i = 0; i < asksData.Length; i++)
        {
            var (price, size) = asksData[i];
            if (size == 0.0)
            {
                asks.Remove(price); // あったら消す、無くても問題ない(なにもしない)
            }
            else
            {
                asks[price] = size;
            }
        }
        if (asks.Any())
        {
            var askFirstPrice = asks.First().Key;
            while (true)
            {
                if (bids.Count == 0) { break; }
                var bidFirstPrice = bids.First().Key;
                if (bidFirstPrice < askFirstPrice) { break; }
                bids.Remove(bidFirstPrice);
            }
        }
    }

    public (long bidsPrice, long asksPrice) GetPrice_SystemSortedDictionary(System.Collections.Generic.SortedDictionary<long, double> bids, System.Collections.Generic.SortedDictionary<long, double> asks)
    {
        double size;

        long bidPrice = default;
        size = 0.0;
        foreach (var item in bids)
        {
            bidPrice = item.Key;
            size += item.Value;
            if (size >= 0.04)
            {
                break;
            }
        }

        long askPrice = default;
        size = 0.0;
        foreach (var item in asks)
        {
            bidPrice = item.Key;
            size += item.Value;
            if (size >= 0.04)
            {
                break;
            }
        }

        return (bidPrice, askPrice);
    }


    [Benchmark]
    public (Gitan.SortedDictionary.SortedDictionary<long, double> bids, Gitan.SortedDictionary.SortedDictionary<long, double> asks) GitanSortedDictionaryBench() => MakeOrderboard_GitanSortedDictionary(_orderBoardDiffs);

    public (Gitan.SortedDictionary.SortedDictionary<long, double> bids, Gitan.SortedDictionary.SortedDictionary<long, double> asks) MakeOrderboard_GitanSortedDictionary(OrderBoardDiff[] diffData)
    {
        var bids = new Gitan.SortedDictionary.SortedDictionary<long, double>(true);
        var asks = new Gitan.SortedDictionary.SortedDictionary<long, double>(false);

        foreach (var diff in diffData)
        {
            lock (_lockObject)
            {
                UpdateOrderBoard_GitanSortedDictionary(bids, asks, diff);
            }
            lock (_lockObject)
            {
#pragma warning disable IDE0042
                var result = GetPrice_GitanSortedDictionary(bids, asks);
#pragma warning restore IDE0042
            }
        }

        return (bids, asks);
    }

    public void UpdateOrderBoard_GitanSortedDictionary(Gitan.SortedDictionary.SortedDictionary<long, double> bids, Gitan.SortedDictionary.SortedDictionary<long, double> asks, OrderBoardDiff diff)
    {
        var bidsData = diff.Bids;
        for (int i = 0; i < bidsData.Length; i++)
        {
            var (price, size) = bidsData[i];
            if (size == 0.0)
            {
                bids.Remove(price); // あったら消す、無くても問題ない(なにもしない)
            }
            else
            {
                bids.AddOrChangeValue(price, size);
            }
        }
        var bidFirstPrice = bids.GetFirstKey();
        if (bidFirstPrice != default)
        {
            asks.RemoveOrUnder(bidFirstPrice);
        }

        var asksData = diff.Asks;
        for (int i = 0; i < asksData.Length; i++)
        {
            var (price, size) = asksData[i];
            if (size == 0.0)
            {
                asks.Remove(price); // あったら消す、無くても問題ない(なにもしない)
            }
            else
            {
                asks.AddOrChangeValue(price, size);
            }
        }
        var askFirstPrice = asks.GetFirstKey();
        if (askFirstPrice != default)
        {
            bids.RemoveOrUnder(askFirstPrice);
        }
    }

    public (long bidsPrice, long asksPrice) GetPrice_GitanSortedDictionary(Gitan.SortedDictionary.SortedDictionary<long, double> bids, Gitan.SortedDictionary.SortedDictionary<long, double> asks)
    {
        double size;

        long bidPrice = default;
        size = 0.0;
        foreach (var item in bids)
        {
            bidPrice = item.Key;
            size += item.Value;
            if (size >= 0.04)
            {
                break;
            }
        }

        long askPrice = default;
        size = 0.0;
        foreach (var item in asks)
        {
            bidPrice = item.Key;
            size += item.Value;
            if (size >= 0.04)
            {
                break;
            }
        }

        return (bidPrice, askPrice);
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
public class SortedDictionaryBench2
{

    public static System.Collections.Generic.KeyValuePair<int, int>[] GetPriceSize()
    {
        var priceSizeList = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>>();

        for (var i = 0; i <= 2; i++)
        {
            for (var j = 0; j < 10000; j++)
            {
                var keyValuePair = new System.Collections.Generic.KeyValuePair<int, int>(j, i);
                priceSizeList.Add(keyValuePair);
            }
        }

        Random r = new();
        var priceSizeArray = priceSizeList.OrderBy(x => r.Next()).ToArray();

        return priceSizeArray;
    }

    static readonly System.Collections.Generic.KeyValuePair<int, int>[] _priceSizeArray = GetPriceSize();

    static readonly System.Collections.Generic.SortedDictionary<int, int> SystemDic = new();
    static readonly Gitan.SortedDictionary.SortedDictionary<int, int> gitanDic = new(false);


    [Benchmark]
    public System.Collections.Generic.SortedDictionary<int,int> SystemSortedDictionaryBench()
    {
        foreach (var priceSize in _priceSizeArray)
        {
            var (price, size) = priceSize;

            if (size == 0)
            {
                SystemDic.Remove(price);
            }
            else
            {
                SystemDic[price] = size;
            }
        }
        return SystemDic;
    }

   [Benchmark]
    public Gitan.SortedDictionary.SortedDictionary<int,int> GitanSortedDictionaryBench()
    {
        foreach (var priceSize in _priceSizeArray)
        {
            var (price, size) = priceSize;

            if (size == 0)
            {
                gitanDic.Remove(price);
            }
            else
            {
                gitanDic[price] = size;
            }
        }
        return gitanDic;
    }


    [Benchmark]
    public void SystemSortedDictionaryBenchForeach()
    {
        int sum = 0;
        foreach (var priceSize in SystemDic)
        {
            sum += priceSize.Value;
        }
    }

    [Benchmark]
    public void GitanSortedDictionaryBenchForeach()
    {
        int sum = 0;
        foreach (var priceSize in gitanDic)
        {
            sum += priceSize.Value;
        }
    }
}


//| Method                             | Runtime  | Mean            | Error          | StdDev         | Ratio | RatioSD |
//|----------------------------------- |--------- |----------------:|---------------:|---------------:|------:|--------:|
//| SystemSortedDictionaryBench        | .NET 6.0 | 8,219,068.69 ns | 163,819.545 ns | 224,238.042 ns |  1.00 |    0.00 |
//| SystemSortedDictionaryBench        | .NET 7.0 | 7,754,178.17 ns | 143,259.915 ns | 140,700.392 ns |  0.94 |    0.03 |
//| SystemSortedDictionaryBench        | .NET 8.0 | 5,324,158.66 ns | 103,940.154 ns | 135,151.560 ns |  0.65 |    0.03 |
//|                                    |          |                 |                |                |       |         |
//| GitanSortedDictionaryBench         | .NET 6.0 | 5,632,193.33 ns | 102,363.615 ns | 143,499.492 ns |  1.00 |    0.00 |
//| GitanSortedDictionaryBench         | .NET 7.0 | 5,154,645.41 ns | 102,636.392 ns | 133,456.301 ns |  0.91 |    0.03 |
//| GitanSortedDictionaryBench         | .NET 8.0 | 4,876,591.21 ns |  95,350.851 ns | 136,749.342 ns |  0.87 |    0.04 |
//|                                    |          |                 |                |                |       |         |
//| SystemSortedDictionaryBenchForeach | .NET 6.0 |        29.05 ns |       0.598 ns |       0.777 ns |  1.00 |    0.00 |
//| SystemSortedDictionaryBenchForeach | .NET 7.0 |        26.77 ns |       0.458 ns |       0.527 ns |  0.92 |    0.03 |
//| SystemSortedDictionaryBenchForeach | .NET 8.0 |        23.93 ns |       0.493 ns |       0.658 ns |  0.83 |    0.03 |
//|                                    |          |                 |                |                |       |         |
//| GitanSortedDictionaryBenchForeach  | .NET 6.0 |        13.62 ns |       0.298 ns |       0.859 ns |  1.00 |    0.00 |
//| GitanSortedDictionaryBenchForeach  | .NET 7.0 |        12.91 ns |       0.445 ns |       1.232 ns |  0.95 |    0.12 |
//| GitanSortedDictionaryBenchForeach  | .NET 8.0 |        10.68 ns |       0.417 ns |       1.149 ns |  0.78 |    0.10 |