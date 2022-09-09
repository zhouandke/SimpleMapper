using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleMapper.Test
{
    // “¿¿µ
    [TestClass]
    public class ClassTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        public ClassTest()
        {

        }

        [TestMethod]
        public void BasicTest()
        {
            var src = new A
            {
                Id = 1,
                Name = "AAAA",
                B = new B() { Age = 999 },
                Point = new Point() { X = 888 }
            };
            var dst = simpleMapper.Map<ADto>(src);
            Assert.AreEqual(src.Id, dst.Id);
            Assert.AreEqual(src.Name, dst.Name);
            Assert.AreEqual(src.B.Age, dst.B.Age);
            Assert.IsNull(dst.B.AgeString);
            Assert.AreEqual(src.Point.X, dst.Point.X);
            Assert.IsNull(dst.Address);
        }

        [TestMethod]
        public void InjectTest()
        {
            // struct
            var src = new Point() { X = 1 };
            var dst = new Location() { X = 100, Y = 100 };
            dst = simpleMapper.InjectFrom(dst, src);
            Assert.AreEqual(1, dst.X);
            Assert.AreEqual(100, dst.Y);

            // class
            var src1 = new
            {
                X = 1,
                Y = (int?)null,
                Point = new Point { X = 1 }
            };

            var dst1 = new InjectTestClass
            {
                X = 2,
                Y = 2,
                Point = new Location() { X = 2 }
            };

            simpleMapper.InjectFrom(dst1, src1);
            Assert.AreEqual(src1.X, dst1.X);
            Assert.AreEqual(0, dst1.Y);
            Assert.AreEqual(src1.Point.X, dst1.Point.X);
        }


        public class InjectTestClass
        {
            public int X { get; set; }

            public int Y { get; set; }

            public Location Point { get; set; }
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

        public struct Location
        {
            public int X { get; set; }

            public int Y { get; set; }
        }
    }


}
