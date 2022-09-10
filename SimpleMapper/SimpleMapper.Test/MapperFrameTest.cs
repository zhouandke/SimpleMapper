using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    // depend none
    [TestClass]
    public class MapperFrameTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        [TestMethod]
        public void CustomMapAndInjectFromTest()
        {
#pragma warning disable CS0618 // 类型或成员已过时
            simpleMapper.SetCustomMap<int, Point>(intValue => new Point() { X = intValue });
#pragma warning restore CS0618 // 类型或成员已过时

            var dst = simpleMapper.Map<Point>(2);
            Assert.AreEqual(2, dst.X);

            dst = simpleMapper.InjectFrom(new Point() { X = 1000000 }, 2);
            Assert.AreEqual(2, dst.X);



            simpleMapper.SetCustomMap<int, Location>((intValue, location) =>
            {
                location.X += intValue;
                return location;
            });

            var location = new Location { X = 10 };
            location = simpleMapper.Map<Location>(2);
            Assert.AreEqual(2, location.X);

            location = new Location { X = 10 };
            location = simpleMapper.InjectFrom(location, 2);
            Assert.AreEqual(12, location.X);
        }

        [TestMethod]
        public void PostActionTest()
        {
            simpleMapper.SetCustomMap<int, Point>((intValue, _) => new Point() { X = intValue });
            simpleMapper.SetPostAction<int, Point>((intValue, point) =>
            {
                if (intValue > 0)
                {
                    point.X += 10000;
                }
                else
                {
                    point.X -= 10000;
                }
                return point;
            });
            var dst = simpleMapper.Map<Point>(2);
            Assert.AreEqual(10002, dst.X);
            dst = simpleMapper.Map<Point>(-2);
            Assert.AreEqual(-10002, dst.X);
        }



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
                Point2 = new Point { X = 1, C = new C { Name = "CCCC" } },
            };
            var dst = simpleMapper.MapAsDeepCopy<ADto>(src);
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
            // No matter how the source changes, the target will not change
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
    }
}
