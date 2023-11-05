■ **Gitan.SortedDictionaryとは**

Gitan.SortedDictionaryは、keyに基づいて並び替えた、キーと値のペアのコレクションです。

プロジェクトURL : [https://github.com/gitan-dev/SortedDictionary](https://github.com/gitan-dev/SortedDictionary)

■ **仕様**

System.Collections.Generic.SortedDictionary<TKey,TValue>の高速版です。

以下の制限事項があります。

 ・TKeyはstruct限定です。

 ・TkeyはIComparable<TKey>を継承している必要があります。

 ・Tkeyの比較条件は、Comparerで指定することはできません。IComparableでの比較のみとなります。


■ **使用方法**

NuGetパッケージ : Gitan.SortedDictionary

NuGetを使用してGitan.SortedDictionaryパッケージをインストールします。

Gitan.SortedDictionaryを使用する方法を以下に記載します。

    using Gitan.SortedDictionary;

    public void AnyFirstTest()
    {
        var dic = new SortedDictionary<double, double>(false);
        KeyValuePair<double, double> item;

        Assert.IsTrue(dic.Any() == false);

        dic.Add(100, 1);
        Assert.IsTrue(dic.Any() == true);
        item = dic.First();
        Assert.IsTrue(item.Key == 100);
        Assert.IsTrue(item.Value == 1);

        dic.Add(200, 2);
        Assert.IsTrue(dic.Any() == true);
        item = dic.First();
        Assert.IsTrue(item.Key == 100);
        Assert.IsTrue(item.Value == 1);

        dic.Add(50, 0.5);
        Assert.IsTrue(dic.Any() == true);
        item = dic.First();
        Assert.IsTrue(item.Key == 50);
        Assert.IsTrue(item.Value == 0.5);

        dic.Remove(50);
        Assert.IsTrue(dic.Any() == true);
        item = dic.First();
        Assert.IsTrue(item.Key == 100);
        Assert.IsTrue(item.Value == 1);


        dic.Remove(100);
        Assert.IsTrue(dic.Any() == true);
        item = dic.First();
        Assert.IsTrue(item.Key == 200);
        Assert.IsTrue(item.Value == 2);

        dic.Remove(200);
        Assert.IsTrue(dic.Any() == false);
    }



■ **パフォーマンス**
   
**SortedDictionary**

public class SortedDictionaryBench
{
    public static KeyValuePair<int, int>[] GetPriceSize()
    {
        var priceSizeList = new List<KeyValuePair<int, int>>();

        for (var i = 0; i <= 2; i++)
        {
            for (var j = 0; j < 10000; j++)
            {
                var keyValuePair = new KeyValuePair<int, int>(j, i);
                priceSizeList.Add(keyValuePair);
            }
        }

        Random r = new();
        var priceSizeArray = priceSizeList.OrderBy(x => r.Next()).ToArray();

        return priceSizeArray;
    }

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

Benchmarkの結果、Gitan.SortedDictionary.SortedDictionaryは
System.Collections.Generic.SortedDictionaryから30％程度速度アップしました。

|                     Method |     Mean |     Error |    StdDev |
|--------------------------- |---------:|----------:|----------:|
|SystemSortedDictionaryBench | 7.554 ms | 0.1474 ms | 0.1448 ms |
| GitanSortedDictionaryBench | 4.854 ms | 0.0521 ms | 0.0487 ms |


|                            Method |            Mean |          Error |        StdDev |
|---------------------------------- |----------------:|---------------:|--------------:|
|       SystemSortedDictionaryBench | 7,514,632.45 ns | 100,913.305 ns | 94,394.368 ns |
|        GitanSortedDictionaryBench | 4,964,345.20 ns |  68,885.469 ns | 61,065.174 ns |
|SystemSortedDictionaryBenchForeach |        25.36 ns |       0.276 ns |      0.244 ns |
| GitanSortedDictionaryBenchForeach |        13.96 ns |       0.390 ns |      1.118 ns |


■ **Api定義**
|コンストラクター|説明|
| -------- | --- |
|SortedDictionary(bool reverse)|SortedDictionary<TKey,TValue>を使用する。reverseをtrueにすると降順になる|
|SortedDictionary(System.Collections.Generic.IDictionary<TKey, TValue> dictionary, bool reverse)|SortedDictionary<Tkey,TValue>から要素をコピーして格納。キーの型の既定の IDictionary<TKey,TValue>を使用する。reverseをtrueにすると降順になる|


|プロパティ|説明|
| ------- | ---- |
|Count|SortedDictionary<TKey,TValue> に格納されているキー/値ペアの数を返します|
|IsReadOnly|falseを返します|
|Compare|指定された値と比較し、小さければ-1,同じなら0,大きければ1を返します|
|TotalCount|SortedDictionary<TKey,TValue> に格納されているキー/値ペアの数を返します|


|メソッド|説明|
| -------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
|Add(TKey key, TValue value)|指定したキーおよび値をSortedDictionary<TKey,TValue> に追加します|
|Add(KeyValuePair<TKey, TValue> item)|指定したキーおよび値をSortedDictionary<TKey,TValue> に追加します|
|TryAdd(TKey key, TValue value)|指定したキーおよび値をSortedDictionary<TKey,TValue> に追加します。失敗時はfalseを返します|
|TryAdd(KeyValuePair<TKey, TValue> item)|指定したキーおよび値をSortedDictionary<TKey,TValue> に追加します。失敗時はfalseを返します|
|AddOrChangeValue(TKey key, TValue value)|指定したキーおよび値をSortedDictionary<TKey,TValue> に追加,変更します|
|AddOrChangeValue(KeyValuePair<TKey, TValue> item)|指定したキーおよび値をSortedDictionary<TKey,TValue> に追加,変更します|
|CopyTo(KeyValuePair<TKey, TValue>[] array)|KeyValuePair<TKey, TValue>をコピーします|
|CopyTo(KeyValuePair<TKey, TValue>[] array, int index)|指定したインデックスを開始位置として、KeyValuePair<TKey, TValue>をコピーします|
|CopyTo(KeyValuePair<TKey, TValue>[] array, int index, int count)|指定したインデックスを開始位置として、KeyValuePair<TKey, TValue>をコピーします|
|RemoveOrUnder(TKey orUnder)|指定したキーより小さいキーはSortedDictionary<TKey,TValue>から削除します|
|Remove(TKey key)|指定したキーを持つ要素をSortedDictionary<TKey,TValue>から削除します|
|Remove(KeyValuePair<TKey, TValue> item)|指定したキーを持つ要素をSortedDictionary<TKey,TValue>から削除します|
|Clear|すべての要素を削除します|
|Contains(KeyValuePair<TKey,TValue> item)|指定したKeyValuePair<TKey,TValue>があり、同じvalueを持つかどうかを返します|
|ContainsKey(TKey key)|指定したkeyと同じ値があるかどうかを返します|
|TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)|指定したキーに関連付けられている値を返します|
|GetEnumerator|foreachの結果を返します|
|Any|コレクションに要素が存在する場合、Trueを返します|
|First|最初のキーのKeyValuePair<TKey, TValue>を返します|
|Last|最後のキーのKeyValuePair<TKey, TValue>を返します|
|GetFirstKey|最初のキーの値を返します|
|GetLastKey|最後のキーの値を返します|
|Find(TKey key)|指定したキーがあるかどうかを返します|
