using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleMapper.Test
{
    // denpend on MapperFrameTest
    [TestClass]
    public class NumberTypeMapperTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        [TestMethod]
        public void BasicTest()
        {
            var src = new A
            {
                Id = 1,
                Name = "Test",
                Age = 33,
                Height = null,
                Weight = 62,
                Death = false,
                IsDelete = 1
            };
            var dst = simpleMapper.Map<ADto>(src);
            Assert.AreEqual(src.Id, dst.Id);
            Assert.AreEqual(src.Name, dst.Name);
            Assert.AreEqual(src.Age, dst.Age.Value);
            Assert.AreEqual(0, dst.Height);
            Assert.AreEqual(src.Weight, dst.Weight);
            Assert.AreEqual(src.Death ? 1 : 0, dst.Death);
            Assert.AreEqual(src.IsDelete == 1 ? true : false, dst.IsDelete);
            Assert.IsNull(dst.Address);
        }


        public class A
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }

            public double? Height { get; set; }

            public float Weight { get; set; }

            public bool Death { get; set; }

            public int IsDelete { get; set; }
        }

        public class ADto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? Age { get; set; }

            public double Height { get; set; }

            public double? Weight { get; set; }

            public decimal Death { get; set; }

            public bool IsDelete { get; set; }

            public string Address { get; set; }
        }
    }
}
