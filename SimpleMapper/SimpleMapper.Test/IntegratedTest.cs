using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    // depend all
    [TestClass]
    public class IntegratedTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        [TestMethod]
        public void ShallowCopyTest()
        {
            var src = new A
            {
                Id = 1,
                B1 = new B { Name = "B1" },
                B2 = new B { Name = "B2" },
                Point1 = new Point { X = 1, C = new C { Name = "CCCC" } },
                Point2 = new Point { X = 1, C = new C { Name = "CCCC" } },
            };
            var dst = simpleMapper.Map<ADto>(src);
            src.Id = 33333;
            src.B1.Name = "B1-333333";
            src.B2.Name = "B1-333333";
            src.Point1.C.Name = "333333";
            src.Point2 = new Point() { X = 333 };

            Assert.AreNotEqual(src.Id, dst.Id);
            Assert.AreSame(src.B1, dst.B1);
            Assert.AreNotEqual(src.B2.Name, dst.B2.Name);
            Assert.AreSame(src.Point1.C, dst.Point1.C);
            Assert.AreNotEqual(src.Point2.X, dst.Point2.X);
        }

        [TestMethod]
        public void DeepCopyTest()
        {
            var src = new A
            {
                Id = 1,
                B1 = new B { Name = "B1" },
                B2 = new B { Name = "B2" },
                Point1 = new Point { X = 1, C = new C { Name = "CCCC" } },
                Point2 = new Point { X = 1 },
            };
            var dst = simpleMapper.MapAsDeepCopy<A>(src);
            Assert.AreEqual(src.Id, dst.Id);
            Assert.AreEqual(src.B1.Name, dst.B1.Name);
            Assert.AreNotSame(src.B1, src.B2);
            Assert.AreEqual(src.B2.Name, dst.B2.Name);
            Assert.AreEqual(src.Point1.X, dst.Point1.X);
            Assert.AreEqual(src.Point1.C.Name, dst.Point1.C.Name);
            Assert.AreNotSame(src.Point1.C, dst.Point1.C);
            Assert.AreEqual(src.Point2.X, dst.Point2.X);

            // No matter how the source changes, the target will not change
            var json0 = System.Text.Json.JsonSerializer.Serialize(dst);
            src.Id = 33333;
            src.B1.Name = "B1-333333";
            src.B1 = new B();
            src.B2.Name = "B1-333333";
            src.B2 = new B();
            src.Point1.C.Name = "333333";
            src.Point1 = new Point();
            src.Point2 = new Point() { X = 333 };
            var json1 = System.Text.Json.JsonSerializer.Serialize(dst);
            Assert.AreEqual(json0, json1);
        }

        public class A
        {
            public int Id { get; set; }

            public B B1 { get; set; }
            public B B2 { get; set; }

            public Point Point1 { get; set; }

            public Point Point2 { get; set; }

        }

        public class ADto
        {
            public int Id { get; set; }

            public B B1 { get; set; }

            public BDto B2 { get; set; }

            public Point Point1 { get; set; }

            public Location Point2 { get; set; }
        }

        public class B
        {
            public string Name { get; set; }
        }

        public class BDto
        {
            public string Name { get; set; }
        }


        public class C
        {
            public string Name { get; set; }
        }


        public struct Point
        {
            public int X { get; set; }

            public C C { get; set; }
        }

        public struct Location
        {
            public int X { get; set; }
        }



        [TestMethod]
        public void NumberTypeEnumerableTest()
        {
            var src = new NumberTypeEnumerableClass
            {
                Ids = new long?[] { 1, (long?)null },
                Colors = new int[] { 1, 2 },
                Scores = new decimal?[] { (decimal?)1.1, (decimal?)null, (decimal?)3.9 }
            };
            var dst = simpleMapper.Map<NumberTypeEnumerableClassDto>(src);
            Assert.AreEqual(src.Ids.Length, dst.Ids.Length);
            Assert.AreEqual(1, dst.Ids[0]);
            Assert.AreEqual(0, dst.Ids[1]);

            Assert.AreEqual(src.Colors.Length, dst.Colors.Length);
            Assert.AreEqual(1, dst.Colors[0]);
            Assert.AreEqual(2, dst.Colors[1]);

            Assert.AreEqual(src.Scores.Length, dst.Scores.Length);
            Assert.AreEqual(1, dst.Scores[0]);
            Assert.AreEqual(0, dst.Scores[1]);
            Assert.AreEqual(3, dst.Scores[2]);
        }

        public class NumberTypeEnumerableClass
        {
            public long?[] Ids { get; set; }

            public int[] Colors { get; set; }

            public decimal?[] Scores { get; set; }
        }

        public class NumberTypeEnumerableClassDto
        {
            public long[] Ids { get; set; }

            public float[] Colors { get; set; }

            public int[] Scores { get; set; }
        }
    }
}
