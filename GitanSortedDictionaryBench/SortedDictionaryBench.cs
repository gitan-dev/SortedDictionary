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


//|                     Method |     Mean |     Error |    StdDev |
//|--------------------------- |---------:|----------:|----------:|
//|SystemSortedDictionaryBench | 7.554 ms | 0.1474 ms | 0.1448 ms |
//| GitanSortedDictionaryBench | 4.854 ms | 0.0521 ms | 0.0487 ms |


//|                            Method |            Mean |          Error |        StdDev |
//|---------------------------------- |----------------:|---------------:|--------------:|
//|       SystemSortedDictionaryBench | 7,514,632.45 ns | 100,913.305 ns | 94,394.368 ns |
//|        GitanSortedDictionaryBench | 4,964,345.20 ns |  68,885.469 ns | 61,065.174 ns |
//|SystemSortedDictionaryBenchForeach |        25.36 ns |       0.276 ns |      0.244 ns |
//| GitanSortedDictionaryBenchForeach |        13.96 ns |       0.390 ns |      1.118 ns |