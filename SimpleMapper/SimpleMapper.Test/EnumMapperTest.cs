using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleMapper.Test
{
    // denpend on MapperFrameTest
    [TestClass]
    public class EnumMapperTest
    {
        ZK.Mapper.SimpleMapper simpleMapper = new ZK.Mapper.SimpleMapper();

        [TestMethod]
        public void BasicTest()
        {
            var src = new A
            {
                EnumToNumber1 = Colors.Red,
                EnumToNumber2 = Colors.Green,
                EnumToNumber3 = null,
                EnumToNumber4 = null,

                EnumToEnum1 = Colors.Red,
                EnumToEnum2 = Colors.Yellow,
                EnumToEnum3 = null,
                EnumToEnum4 = null,

                ToEnum1 = 8,
                ToEnum2 = null,
                ToEnum3 = null,
                ToEnum4 = "red",
                ToEnum5 = "xxxxxxx",
                ToEnum6 = 2f,
                ToEnum7 = 2.2,
                ToEnum8 = 3,
                ToEnum9 = true,
            };
            var dst = simpleMapper.Map<ADto>(src);
            Assert.AreEqual(1, dst.EnumToNumber1);
            Assert.AreEqual(2, dst.EnumToNumber2);
            Assert.IsNull(dst.EnumToNumber3);
            Assert.AreEqual(0, dst.EnumToNumber4);

            Assert.AreEqual(Sport.Walk, dst.EnumToEnum1);
            Assert.AreEqual(4, (int)dst.EnumToEnum2);
            Assert.IsNull(dst.EnumToEnum3);
            Assert.AreEqual(Sport.Unknown, dst.EnumToEnum4);

            Assert.AreEqual(8, (int)dst.ToEnum1);
            Assert.AreEqual(Colors.Unknown, dst.ToEnum2);
            Assert.IsNull(dst.ToEnum3);
            Assert.AreEqual(Colors.Red, dst.ToEnum4);
            Assert.IsNull(dst.ToEnum5);
            Assert.AreEqual(Colors.Green, dst.ToEnum6);
            Assert.AreEqual(Colors.Unknown, dst.ToEnum7);
            Assert.AreEqual(Colors.Blue, dst.ToEnum8);
        }

        public class A
        {
            public Colors EnumToNumber1 { get; set; }
            public Colors? EnumToNumber2 { get; set; }
            public Colors? EnumToNumber3 { get; set; }
            public Colors? EnumToNumber4 { get; set; }

            public Colors EnumToEnum1 { get; set; }
            public Colors? EnumToEnum2 { get; set; }
            public Colors? EnumToEnum3 { get; set; }
            public Colors? EnumToEnum4 { get; set; }

            public int ToEnum1 { get; set; }
            public int? ToEnum2 { get; set; }
            public int? ToEnum3 { get; set; }
            public string ToEnum4 { get; set; }
            public string ToEnum5 { get; set; }
            public float ToEnum6 { get; set; }
            public double ToEnum7 { get; set; }
            public decimal ToEnum8 { get; set; }
            public bool ToEnum9 { get; set; }
        }


        public class ADto
        {
            public int EnumToNumber1 { get; set; }
            public int? EnumToNumber2 { get; set; }
            public int? EnumToNumber3 { get; set; }
            public int EnumToNumber4 { get; set; }

            public Sport EnumToEnum1 { get; set; }
            public Sport? EnumToEnum2 { get; set; }
            public Sport? EnumToEnum3 { get; set; }
            public Sport EnumToEnum4 { get; set; }

            public Colors ToEnum1 { get; set; }
            public Colors ToEnum2 { get; set; }
            public Colors? ToEnum3 { get; set; }
            public Colors ToEnum4 { get; set; }
            public Colors? ToEnum5 { get; set; }
            public Colors ToEnum6 { get; set; }
            public Colors ToEnum7 { get; set; }
            public Colors ToEnum8 { get; set; }
            public Colors ToEnum9 { get; set; }
        }

        public enum Colors
        {
            Unknown = 0,
            Red = 1,
            Green = 2,
            Blue = 3,
            Yellow = 4,
        }


        public enum Sport
        {
            Unknown = 0,
            Walk = 1,
            Run = 2,
            Ride = 3,
        }

    }
}
