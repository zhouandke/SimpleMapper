using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    [TestClass]
    public class DictionaryTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        [TestMethod]
        public void BasicTest()
        {
            var src = new A
            {
                Dict1 = new Dictionary<int, int>()
                {
                    { 1, 1 }
                },
                Dict2 = new Dictionary<int, string>()
                {
                    { 1, "1" },
                    { 2, "aaaa" }
                },
                Dict3 = new Dictionary<int, int>()
                {
                    { 3, 3 }
                }
            };
            var dst = simpleMapper.Map<ADto>(src);
            Assert.AreSame(src.Dict1, dst.Dict1);
            Assert.AreEqual(src.Dict2.Count, dst.Dict2.Count);
            Assert.AreEqual(1, dst.Dict2[1]);
            Assert.AreEqual(0, dst.Dict2[2]);
            Assert.IsNull(dst.Dict3);
        }

        public class A
        {
            public Dictionary<int, int> Dict1 { get; set; }

            public Dictionary<int, string> Dict2 { get; set; }

            public Dictionary<int, int> Dict3 { get; set; }
        }


        public class ADto
        {
            public Dictionary<int, int> Dict1 { get; set; }

            public Dictionary<int, int> Dict2 { get; set; }

            public Dictionary<string, int> Dict3 { get; set; }
        }
    }
}
