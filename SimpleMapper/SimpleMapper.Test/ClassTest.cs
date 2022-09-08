using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleMapper.Test
{
    [TestClass]
    public class ClassTest
    {
        [TestMethod]
        public void ClassBasicTest()
        {
            var src = new A
            {
                Id = 1,
                Name = "AAAA",
                B = new B() { Age = 999 },
                Point = new Point() { X = 888 }

            };
            var dst = ZK.Mapper.SimpleMapper.Default.Map<ADto>(src);
            Assert.AreEqual(src.Id, dst.Id);
            Assert.AreEqual(src.Name, dst.Name);
            Assert.AreEqual(src.B.Age, dst.B.Age);
            Assert.IsNull(dst.B.AgeString);
            //Assert.AreEqual(src.Point.X, dst.Point.X);
            Assert.IsNull(dst.Address);
        }


        public class A
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public B B { get; set; }

            public Point Point { get; set; }
        }

        public class ADto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public BDto B { get; set; }

            public PointDto Point { get; set; }

            public string Address { get; set; }
        }

        public class B
        {
            public int Age { get; set; }
        }

        public class BDto : B
        {
            public string AgeString { get; set; }
        }

        public struct Point
        {
            public int X { get; set; }
        }

        public struct PointDto
        {
            public int X { get; set; }
        }
    }


}
