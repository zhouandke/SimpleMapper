using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SimpleMapper.Test
{
    // denpend on BasicTest
    [TestClass]
    public class EnumerableMapperTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        [TestMethod]
        public void BasicTest()
        {
            var src = new A
            {
                ToArray = new List<int> { 1, 2, 3 },
                ToList = new int[] { 1, 2, 3 },
                ToCollection = new[] { new Address() { Id = 1 } },
                ToHashSet = new int[] { 1, 2, 1, 2 },
                ListToArray = new List<Address> { new Address(), new Address() }
            };
            var dst = simpleMapper.Map<ADto>(src);
            Assert.AreEqual(src.ToArray.Count, dst.ToArray.Length);
            Assert.AreEqual(src.ToArray[0], dst.ToArray[0]);
            Assert.AreEqual(src.ToArray[1], dst.ToArray[1]);
            Assert.AreEqual(src.ToArray[2], dst.ToArray[2]);

            Assert.AreEqual(src.ToList.Length, dst.ToList.Count);
            Assert.AreEqual(src.ToList[0], dst.ToList[0]);
            Assert.AreEqual(src.ToList[1], dst.ToList[1]);
            Assert.AreEqual(src.ToList[2], dst.ToList[2]);

            Assert.AreEqual(src.ToCollection.Length, dst.ToCollection.Count);
            Assert.AreEqual(src.ToCollection[0].Id, dst.ToCollection[0].Id);

            Assert.AreEqual(src.ToHashSet.Distinct().Count(), dst.ToHashSet.Count);
            Assert.IsTrue(dst.ToHashSet.Contains(1));
            Assert.IsTrue(dst.ToHashSet.Contains(2));

            Assert.AreEqual(src.ListToArray.Count, dst.ListToArray.Length);
            Assert.AreEqual(0, dst.ListToArray[0]);
            Assert.AreEqual(0, dst.ListToArray[1]);
        }

        public class A
        {
            public List<int> ToArray { get; set; }
            public int[] ToList { get; set; }
            public Address[] ToCollection { get; set; }
            public int[] ToHashSet { get; set; }
            public List<Address> ListToArray { get; set; }
        }


        public class ADto
        {
            public int[] ToArray { get; set; }
            public List<int> ToList { get; set; }
            public Collection<Person> ToCollection { get; set; }
            public HashSet<int> ToHashSet { get; set; }
            public int[] ListToArray { get; set; }
        }

        public class Address
        {
            public int Id { get; set; }
        }

        public class Person
        {
            public int Id { get; set; }
        }
    }
}
