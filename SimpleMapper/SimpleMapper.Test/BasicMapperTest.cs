using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    // depend none
    [TestClass]
    public class BasicMapperTest
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


        public struct Point
        {
            public int X { get; set; }
        }

        public struct Location
        {
            public int X { get; set; }
        }

    }
}
