using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Gitan.SortedDictionary.Tests;

[TestClass()]
public class SortedDictionaryTests
{

    [TestMethod()]
    public void DictinaryTests()
    {
        Dictionary<int, int> dic = new()
        {
            [0] = 1,
            [1] = 2,
            [2] = 3,
            [3] = 4,
            [4] = 5,
        };

        var gitanDic1 = new SortedDictionary<int, int>(false)
        {
            [0] = 1,
            [1] = 2,
            [2] = 3,
            [3] = 4,
            [4] = 5,
        };

        var gitanDic2 = new SortedDictionary<int, int>(dic, false);

        Assert.IsTrue(gitanDic1.SequenceEqual(gitanDic2));       
    }

#pragma warning disable IDE0028
    [TestMethod()]
    public void ReverseTests()
    {
        var gitanDicFalse = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        gitanDicFalse[1] = 11;
        gitanDicFalse[3] = 33;
        gitanDicFalse[2] = 22;

        var list = gitanDicFalse.ToList();
        Assert.IsTrue(list.Count == 3);
        Assert.IsTrue(list[0].Key == 1);
        Assert.IsTrue(list[1].Key == 2);
        Assert.IsTrue(list[2].Key == 3);

        var gitanDicTrue = new Gitan.SortedDictionary.SortedDictionary<int, int>(true);
        gitanDicTrue[1] = 11;
        gitanDicTrue[3] = 33;
        gitanDicTrue[2] = 22;

        list = gitanDicTrue.ToList();
        Assert.IsTrue(list.Count == 3);
        Assert.IsTrue(list[0].Key == 3);
        Assert.IsTrue(list[1].Key == 2);
        Assert.IsTrue(list[2].Key == 1);
    }

    [TestMethod()]
    public void AnyFirstTest()
    {
        var dic = new SortedDictionary<double, double>(false);
        KeyValuePair<double, double> item;
        KeyValuePair<double, double> pair = new(50, 0.5);

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

        dic.Add(pair);
        Assert.IsTrue(dic.Any() == true);
        item = dic.First();
        Assert.IsTrue(item.Key == 50);
        Assert.IsTrue(item.Value == 0.5);

        dic.Remove(pair);
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

    public static KeyValuePair<int, int>[] GetSouceArray()
    {
        var souce = new List<KeyValuePair<int, int>>();

        for (var i = 0; i <= 2; i++)
        {
            for (var j = 0; j < 10000; j++)
            {
                var keyValuePair = new KeyValuePair<int, int>(j, i);
                souce.Add(keyValuePair);
            }
        }

        Random r = new();
        var souceArray = souce.OrderBy(x => r.Next()).ToArray();

        return souceArray;
    }

    [TestMethod()]
    public void DictionaryComparisonTests()
    {
        var souceArray = GetSouceArray();

        var sysDic = new System.Collections.Generic.SortedDictionary<int, int>();
        var giatnDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);

        foreach (var keyValuePair in souceArray)
        {
            var (key, value) = keyValuePair;

            if (value == 0)
            {
                sysDic.Remove(key);
                giatnDic.Remove(key);
            }
            else
            {
                sysDic[key] = value;
                giatnDic[key] = value;
            }            
        }

        Assert.IsTrue(sysDic.SequenceEqual(giatnDic));
        Assert.IsTrue(sysDic.Count == giatnDic.Count);

        var sysDicArray = sysDic.ToArray();
        var gitanDicArray = giatnDic.ToArray();

        Assert.IsTrue(sysDicArray.SequenceEqual(gitanDicArray));
    }

    [TestMethod]
    public void TotalCountTests()
    {
        SortedDictionary<int, int> gitanDic = new SortedDictionary<int, int>(false);

        gitanDic[0] = 1;
        gitanDic[1] = 2;
        gitanDic[2] = 3;
        gitanDic[3] = 4;
        gitanDic[4] = 5;

       var totalCount = gitanDic.TotalCount();
       Assert.IsTrue (totalCount == gitanDic.Count);
    }

    [TestMethod()]
    public void AddTests()
    {
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);

        var result = gitanDic.TryAdd(1, 100);
        Assert.IsTrue(result);
        result = gitanDic.TryAdd(1, 100);
        Assert.IsFalse(result);

        try
        {
            gitanDic.Add(1, 100);
            Assert.Fail();
        }
        catch { }

        KeyValuePair<int, int> pair = new(2, 200);
        result = gitanDic.TryAdd(pair);
        Assert.IsTrue(result);
        result = gitanDic.TryAdd(pair);
        Assert.IsFalse(result);

        var value = gitanDic.Find(1);
        Assert.IsTrue(value == 100);

        try
        {
            gitanDic.Add(pair);
            Assert.Fail();
        }
        catch { }

        value = gitanDic.Find(2);
        Assert.IsTrue(value == 200);
    }

    [TestMethod()]
    public void AddOrChangeValueTests()
    {
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        gitanDic[1] = 100;

        Assert.IsTrue(gitanDic[1] == 100);

        gitanDic.AddOrChangeValue(1, 200);
        Assert.IsTrue(gitanDic[1] == 200);

        KeyValuePair<int, int> pair = new(1, 300);
        gitanDic.AddOrChangeValue(pair);
        Assert.IsTrue(gitanDic[1] == 300);
    }

    [TestMethod()]
    public void CopyToTests()
    {
        var systemDic = new System.Collections.Generic.SortedDictionary<int, int>();
        systemDic[1] = 11;
        systemDic[3] = 33;
        systemDic[2] = 22;
        KeyValuePair<int, int>[] buffer = new KeyValuePair<int, int>[100];

        int offset = 0;
        systemDic.CopyTo(buffer, offset);
        offset += systemDic.Count;

        Assert.IsTrue(buffer[0].Key == 1);
        Assert.IsTrue(buffer[0].Value == 11);

        Assert.IsTrue(buffer[1].Key == 2);
        Assert.IsTrue(buffer[1].Value == 22);

        Assert.IsTrue(buffer[2].Key == 3);
        Assert.IsTrue(buffer[2].Value == 33);

        systemDic.CopyTo(buffer, offset);
        offset += systemDic.Count;

        Assert.IsTrue(buffer[3].Key == 1);
        Assert.IsTrue(buffer[3].Value == 11);

        Assert.IsTrue(buffer[4].Key == 2);
        Assert.IsTrue(buffer[4].Value == 22);

        Assert.IsTrue(buffer[5].Key == 3);
        Assert.IsTrue(buffer[5].Value == 33);

        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        gitanDic[1] = 11;
        gitanDic[3] = 33;
        gitanDic[2] = 22;
        KeyValuePair<int, int>[] gitanBuffer = new KeyValuePair<int, int>[100];

        int gitanOffset = 0;
        gitanDic.CopyTo(gitanBuffer, gitanOffset);
        gitanOffset += gitanDic.Count;

        Assert.IsTrue(gitanBuffer[0].Key == 1);
        Assert.IsTrue(gitanBuffer[0].Value == 11);

        Assert.IsTrue(gitanBuffer[1].Key == 2);
        Assert.IsTrue(gitanBuffer[1].Value == 22);

        Assert.IsTrue(gitanBuffer[2].Key == 3);
        Assert.IsTrue(gitanBuffer[2].Value == 33);

        gitanDic.CopyTo(gitanBuffer, gitanOffset);
        gitanOffset += gitanDic.Count;

        Assert.IsTrue(gitanBuffer[3].Key == 1);
        Assert.IsTrue(gitanBuffer[3].Value == 11);

        Assert.IsTrue(gitanBuffer[4].Key == 2);
        Assert.IsTrue(gitanBuffer[4].Value == 22);

        Assert.IsTrue(gitanBuffer[5].Key == 3);
        Assert.IsTrue(gitanBuffer[5].Value == 33);

        Assert.IsTrue(offset == gitanOffset);
        Assert.IsTrue(buffer.SequenceEqual(gitanBuffer));

        var gitanDicArrayOnly = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        gitanDicArrayOnly[1] = 11;
        gitanDicArrayOnly[3] = 33;
        gitanDicArrayOnly[2] = 22;
        KeyValuePair<int, int>[] gitanBufferArrayOnly = new KeyValuePair<int, int>[100];

        int gitanOffset2 = 0;
        gitanDicArrayOnly.CopyTo(gitanBufferArrayOnly);
        gitanOffset2 += gitanDicArrayOnly.Count;

        Assert.IsTrue(gitanBufferArrayOnly[0].Key == 1);
        Assert.IsTrue(gitanBufferArrayOnly[0].Value == 11);

        Assert.IsTrue(gitanBufferArrayOnly[1].Key == 2);
        Assert.IsTrue(gitanBufferArrayOnly[1].Value == 22);

        Assert.IsTrue(gitanBufferArrayOnly[2].Key == 3);
        Assert.IsTrue(gitanBufferArrayOnly[2].Value == 33);

        var totalCount = gitanDicArrayOnly.TotalCount();

        gitanDicArrayOnly.CopyTo(gitanBufferArrayOnly, gitanOffset2, totalCount);
        gitanOffset2 += gitanDicArrayOnly.Count;

        Assert.IsTrue(gitanBufferArrayOnly[3].Key == 1);
        Assert.IsTrue(gitanBufferArrayOnly[3].Value == 11);

        Assert.IsTrue(gitanBufferArrayOnly[4].Key == 2);
        Assert.IsTrue(gitanBufferArrayOnly[4].Value == 22);

        Assert.IsTrue(gitanBufferArrayOnly[5].Key == 3);
        Assert.IsTrue(gitanBufferArrayOnly[5].Value == 33);

        Assert.IsTrue(offset == gitanOffset2);
        Assert.IsTrue(gitanBuffer.SequenceEqual(gitanBufferArrayOnly));
    }

    [TestMethod()]
    public void RemoveOrUnderTests()
    {
        var souceArray = GetSouceArray();
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);

        foreach (var keyValuePair in souceArray)
        {
            var (key, value) = keyValuePair;
            gitanDic[key] = value;
        }

        gitanDic.RemoveOrUnder(5000);

        for (int falseValue = 0; falseValue <= 5000; falseValue++)
        {
            var resultGitanDicContainsKey = gitanDic.ContainsKey(falseValue);
            Assert.IsFalse(resultGitanDicContainsKey);
        }

        for (int tureValue = 5001; tureValue < 10000; tureValue++)
        {
            var resultGitanDicContainsKey = gitanDic.ContainsKey(tureValue);
            Assert.IsTrue(resultGitanDicContainsKey == true);
        }

        var first = gitanDic.First();
        Assert.IsTrue(first.Key == 5001);

        var last = gitanDic.Last();
        Assert.IsTrue(last.Key == 9999);
    }

    [TestMethod()]
    public void ContainsTests()
    {
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        KeyValuePair<int, int> pair = new(0, 1000);
        KeyValuePair<int, int> pair2 = new(0, 500);
        KeyValuePair<int, int> pair3 = new(1, 1000);

        gitanDic.Add(pair);
        var result = gitanDic.Contains(pair);
        Assert.IsTrue(result);

        var result2 = gitanDic.Contains(pair2);
        Assert.IsFalse(result2);

        var result3 = gitanDic.Contains(pair3);
        Assert.IsFalse(result3);
    }

    [TestMethod()]
    public void ContainsKeyTests()
    {
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        gitanDic[1] = 11;
        gitanDic[3] = 33;
        gitanDic[2] = 22;

        var gitanContainsKeyTrue = gitanDic.ContainsKey(1);
        Assert.IsTrue(gitanContainsKeyTrue);

        var gitanContainsKeyFalse = gitanDic.ContainsKey(4);
        Assert.IsFalse(gitanContainsKeyFalse);
    }

    [TestMethod()]
    public void TryGetValueTests()
    {
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        gitanDic[1] = 11;
        gitanDic[3] = 33;
        gitanDic[2] = 22;

        var gitanTryGetValueTrue = gitanDic.TryGetValue(1, out var value1);
        Assert.IsTrue(gitanTryGetValueTrue);
        var result = gitanDic.Find(1);
        Assert.IsTrue(result == value1);

        var gitanTryGetValuefalse = gitanDic.TryGetValue(4, out var value2);
        Assert.IsFalse(gitanTryGetValuefalse);
        Assert.IsTrue(0 == value2);
    }

    [TestMethod()]
    public void ClearContainsKeyTryGetValueTests()
    {
        var list = new List<KeyValuePair<int, int>>
        {
            new KeyValuePair<int, int>(1, 11),
            new KeyValuePair<int, int>(2, 22),
            new KeyValuePair<int, int>(3, 33),
        };

        var systemDic = new System.Collections.Generic.SortedDictionary<int, int>();
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);

        foreach (var keyValuePair_a in list)
        {
            var (key, value) = keyValuePair_a;

            if (value == 0)
            {
                systemDic.Remove(key);
                gitanDic.Remove(key);
            }
            else
            {
                systemDic[key] = value;
                gitanDic[key] = value;
            }
            foreach (var keyValuePair_b in list)
            {
                (key, value) = keyValuePair_b;

                var resultSystemDicContainsKey = systemDic.ContainsKey(key);
                var resultGitanDicContainsKey = gitanDic.ContainsKey(key);

                Assert.IsTrue(resultSystemDicContainsKey == resultGitanDicContainsKey);

                var resultSystemDicTryGetValue = systemDic.TryGetValue(key, out var size1);
                var resultGitanDicTryGetValue = gitanDic.TryGetValue(key, out var size2);

                Assert.IsTrue(resultSystemDicTryGetValue == resultGitanDicTryGetValue);

                if (resultSystemDicTryGetValue == true)
                {
                    Assert.IsTrue(size1 == size2);
                }
            }
        }
        systemDic.Clear();
        gitanDic.Clear();

        Assert.IsTrue(systemDic.SequenceEqual(gitanDic));
    }

    [TestMethod()]
    public void FirstLastTests()
    {
        var systemDic = new System.Collections.Generic.SortedDictionary<int, int>();
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);
        var souceArray = GetSouceArray();

        foreach (var keyValuePair in souceArray)
        {
            var (key, value) = keyValuePair;

            if (value == 0)
            {
                systemDic.Remove(key);
                gitanDic.Remove(key);
            }
            else
            {
                systemDic[key] = value;
                gitanDic[key] = value;
            }

            Assert.IsTrue(systemDic.Count == gitanDic.Count);

            if (systemDic.Any())
            {
                var sysFirst = systemDic.First();
                var sysLast = systemDic.Last();
                var gitanFirst = gitanDic.First();
                var gitanLast = gitanDic.Last();

                Assert.IsTrue(sysFirst.Equals(gitanFirst));
                Assert.IsTrue(sysLast.Equals(gitanLast));

                var firstKey = gitanDic.GetFirstKey();
                var lastKey = gitanDic.GetLastKey();

                Assert.IsTrue(gitanFirst.Key == firstKey);
                Assert.IsTrue(gitanLast.Key == lastKey);
            }
        }
    }
#pragma warning restore IDE0028

    [TestMethod()]
    public void IEnumerableTests()
    {
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);

        gitanDic[0] = 100;
        gitanDic[1] = 101;
        gitanDic[2] = 102;
        
        IEnumerable<KeyValuePair<int, int>> ie = gitanDic;

        foreach (var item in ie)
        {
            System.Diagnostics.Debug.WriteLine(item.Key);
        }
    } 
    
    [TestMethod()]
    public void IEnumerableLegacyTests()
    {
        var gitanDic = new Gitan.SortedDictionary.SortedDictionary<int, int>(false);

        gitanDic[0] = 100;
        gitanDic[1] = 101;
        gitanDic[2] = 102;
        
        IEnumerable ie = gitanDic;

        foreach (var item in ie)
        {
            var item2 = (KeyValuePair<int, int>)item;
            System.Diagnostics.Debug.WriteLine(item2.Key);
        }
    }
}