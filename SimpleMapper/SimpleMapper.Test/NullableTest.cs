using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    [TestClass]
    public class NullableTest
    {
        [TestMethod]
        public void NullableBasicTest()
        {
            var src = new A
            {
                Point1 = new Point { X = 1 },
                Point2 = null,
                Point3 = new Point { X = 3 },
            };
            var dst = ZK.Mapper.SimpleMapper.Default.Map<ADto>(src);
            Assert.AreEqual(src.Point1.Value.X, dst.Point1.X);
            Assert.AreEqual(0, dst.Point2.X);
            Assert.IsNotNull(dst.Point3);
            Assert.AreEqual(src.Point3.X, dst.Point3.Value.X);

            //Point? p1 = null;
            //Point p2 = new Point() { X = 2 };
            //Point? p3 = new Point() { X = 3 };
            //ZK.Mapper.SimpleMapper.Default.Inject(p2, p1);
            //Assert.AreEqual(2, p2.X);

            //ZK.Mapper.SimpleMapper.Default.Inject(p1, p2);
            //Assert.IsNull(p1);

            // TODO
            //ZK.Mapper.SimpleMapper.Default.Inject(p2, p3);
            //Assert.AreEqual(p3.Value.X, p2.X);


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
