using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gitan.SortedDictionary.Tests
{
    [TestClass()]
    public class SortedDictionaryBenchTests
    {
        static readonly SortedDictionaryBench2 bench = new();

        [TestMethod()]
        public void SortedDictionaryBench2Test()
        {
            var sysDic = bench.SystemSortedDictionaryBench();
            var gitanDic = bench.GitanSortedDictionaryBench();

            var sysDicList = new List<(int, int)>();
            foreach ( var item in sysDic)
            {
                sysDicList.Add((item.Key, item.Value));
            }

            var gitanDicList = new List<(int, int)>();
            foreach (var item in gitanDic)
            {
                gitanDicList.Add((item.Key, item.Value));
            }

            Assert.IsTrue(sysDicList.SequenceEqual(gitanDicList));

            bench.SystemSortedDictionaryBenchForeach();
            bench.GitanSortedDictionaryBenchForeach();
        }
    }
}