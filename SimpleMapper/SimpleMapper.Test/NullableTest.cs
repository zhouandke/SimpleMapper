using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    [TestClass]
    public class NullableTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        [TestMethod]
        public void BasicTest()
        {
            var src = new A
            {
                Point1 = new Point { X = 1 },
                Point2 = null,
                Point3 = new Point { X = 3 },
            };
            var dst = simpleMapper.Map<ADto>(src);
            Assert.AreEqual(src.Point1.Value.X, dst.Point1.X);
            Assert.AreEqual(0, dst.Point2.X);
            Assert.IsNotNull(dst.Point3);
            Assert.AreEqual(src.Point3.X, dst.Point3.Value.X);
        }

        public class A
        {
            public Point? Point1 { get; set; }

            public Point? Point2 { get; set; }

            public Point Point3 { get; set; }
        }


        public class ADto
        {
            public Point Point1 { get; set; }

            public Point Point2 { get; set; }

            public Point? Point3 { get; set; }
        }

        public struct Point
        {
            public int X { get; set; }
        }

    }
}
