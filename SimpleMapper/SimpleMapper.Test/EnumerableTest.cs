using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    [TestClass]
    public class EnumerableTest
    {
        [TestMethod]
        public void EnumerableBasicTest()
        {
            var src = new A
            {
                ToArray = new[] { 1, 2, 3 },
                ToList = new int?[] { 1, (int?)null, 3 },
                ToCollection = new[] { new Address() { Id = 1, DetailAddress = "China Sichuan" } },
                ToHashSet = new int[] { 1, 2, 1, 2 },
                ListToArray = new List<Address> { new Address(), new Address() }
            };
            var dst = ZK.Mapper.SimpleMapper.Default.Map<ADto>(src);
            Assert.AreEqual(src.ToArray.Length, dst.ToArray.Length);
            Assert.AreEqual(src.ToArray[0], dst.ToArray[0]);
            Assert.AreEqual(src.ToArray[1], dst.ToArray[1]);
            Assert.AreEqual(src.ToArray[2], dst.ToArray[2]);
                   
            Assert.AreEqual(src.ToList.Length, dst.ToList.Count);
            Assert.AreEqual(1, dst.ToList[0]);
            Assert.AreEqual(0, dst.ToList[1]);
            Assert.AreEqual(3, dst.ToList[2]);
                   
            Assert.AreEqual(src.ToCollection.Length, dst.ToCollection.Count);
            Assert.AreEqual(src.ToCollection[0].Id, dst.ToCollection[0].Id);
            Assert.AreEqual(null, dst.ToCollection[0].Name);
                   
            Assert.AreEqual(2, dst.ToHashSet.Count);
            Assert.IsTrue(dst.ToHashSet.Contains(1));
            Assert.IsTrue(dst.ToHashSet.Contains(2));

            Assert.AreEqual(src.ListToArray.Count, dst.ListToArray.Length);
            Assert.AreEqual(0, dst.ListToArray[0]);
            Assert.AreEqual(0, dst.ListToArray[1]);
        }

        public class A
        {
            public int[] ToArray { get; set; }
            public int?[] ToList { get; set; }
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

            public string DetailAddress { get; set; }
        }

        public class Person
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
