using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    [TestClass]
    public class CustomAndInjectFromTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();


        [TestMethod]
        public void BasicTest()
        {
            simpleMapper.SetCustomMap<int, Point>(intValue => new Point() { X = intValue });
            simpleMapper.SetCustomMap<int, Location>((intValue, location) =>
            {
                location.X += intValue;
                return location;
            });

            var dst = simpleMapper.Map<Point>(2);
            Assert.AreEqual(2, dst.X);

            dst = simpleMapper.InjectFrom(new Point() { X = 1000000 }, 2);
            Assert.AreEqual(2, dst.X);



            var location = new Location { X = 10 };
            location = simpleMapper.Map<Location>(2);
            Assert.AreEqual(2, location.X);

            location = new Location { X = 10 };
            location = simpleMapper.InjectFrom(location, 2);
            Assert.AreEqual(12, location.X);
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
