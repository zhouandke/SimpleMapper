using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleMapper.Test
{
    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public void PrimitiveTypeTest()
        {
            var a = new A
            {
                Id = 1,
                Age = 33,
                Height = null,
                Weight = 62,
                Death = false
            };
            var b = ZK.Mapper.SimpleMapper.Default.Map<ADto>(a);
            Assert.AreEqual(a.Id, b.Id);
            Assert.AreEqual(a.Age, b.Age);
            Assert.AreEqual(0, b.Height);
            Assert.AreEqual(a.Weight, b.Weight);
            Assert.AreEqual(a.Death ? 1 : 0, b.Death);
        }
    }



    public class A
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public int? Height { get; set; }

        public float Weight { get; set; }

        public bool Death { get; set; }

    }


    public class ADto
    {
        public int Id { get; set; }

        public int? Age { get; set; }

        public double Height { get; set; }

        public double? Weight { get; set; }

        public decimal Death { get; set; }
    }
}
