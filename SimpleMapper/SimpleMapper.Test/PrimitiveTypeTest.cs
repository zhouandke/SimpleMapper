using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleMapper.Test
{
    [TestClass]
    public class PrimitiveTypeTest
    {
        [TestMethod]
        public void PrimitiveTypeBasicTest()
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
            var dst = ZK.Mapper.SimpleMapper.Default.Map<ADto>(src);
            Assert.AreEqual(src.Id, dst.Id);
            Assert.AreEqual(src.Name, dst.Name);
            Assert.AreEqual(src.Age, dst.Age.Value);
            Assert.AreEqual(0, dst.Height);
            Assert.AreEqual(src.Weight, dst.Weight);
            Assert.AreEqual(src.Death ? 1 : 0, dst.Death);
            Assert.AreEqual(src.IsDelete == 1 ? true : false, dst.IsDelete);
            Assert.IsNull(dst.Address);
        }

        [TestMethod]
        public void PrimitiveTypeEnumerableTest()
        {
            decimal k = 1;
            var src = new B
            {
                Ids = new long?[] { 1, (long?)null },
                Colors = new int[] { 1, 2 },
                Scores = new decimal?[] { (decimal?)1.1, (decimal?)null, (decimal?)3.9 }
            };
            var dst = ZK.Mapper.SimpleMapper.Default.Map<BDto>(src);
            Assert.AreEqual(src.Ids.Length, dst.Ids.Length);
            Assert.AreEqual(1, dst.Ids[0]);
            Assert.AreEqual(0, dst.Ids[1]);

            Assert.AreEqual(src.Colors.Length, dst.Colors.Length);
            Assert.AreEqual(1, dst.Colors[0]);
            Assert.AreEqual(2, dst.Colors[1]);

            Assert.AreEqual(src.Scores.Length, dst.Scores.Length);
            Assert.AreEqual(1, dst.Scores[0]);
            Assert.AreEqual(0, dst.Scores[1]);
            // TODO: Bug?
            //var d = (decimal?)3.9;
            //var v1 = (int)d; // 3
            //var v2 = System.Convert.ChangeType(d, typeof(int)); //4
            Assert.AreEqual(4, dst.Scores[1]);

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

        public class B
        {
            public long?[] Ids { get; set; }

            public int[] Colors { get; set; }

            public decimal?[] Scores { get; set; }
        }

        public class BDto
        {
            public long[] Ids { get; set; }

            public float[] Colors { get; set; }

            public int[] Scores { get; set; }
        }

        public struct PointDto
        {
            public int X { get; set; }
        }
    }


}
