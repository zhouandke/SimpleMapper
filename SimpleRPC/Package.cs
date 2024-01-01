using System;
using System.Collections.Generic;

namespace SimpleRPC
{
    public struct PackageHead
    {
        public const uint HeadMarkValue = 0x5F5F5F5F;
        public const ushort TailMarkValue = 0xF5F5;
        public const int HeaderSize = 16;

        public uint HeadMark;
        public uint CmdId;
        public uint BodySize;
        public byte Type;
        /// <summary>
        /// 0 表示无异常
        /// </summary>
        public byte ResponseCode;
        public ushort TailMark;

        public byte[] ToBytes()
        {
            var list = new List<byte>(HeaderSize);
            list.AddRange(BitConverter.GetBytes(HeadMark));
            list.AddRange(BitConverter.GetBytes(CmdId));
            list.AddRange(BitConverter.GetBytes(BodySize));
            list.Add(Type);
            list.Add(ResponseCode);
            list.AddRange(BitConverter.GetBytes(TailMark));
            return list.ToArray();
        }

        unsafe public static PackageHead Convert(byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                PackageHead requestHead = new PackageHead
                {
                    HeadMark = *(uint*)(ptr + offset),
                    CmdId = *(uint*)(ptr + offset + 4),
                    BodySize = *(uint*)(ptr + offset + 8),
                    Type = *(ptr + offset + 12),
                    ResponseCode = *(ptr + offset + 13),
                    TailMark = *(ushort*)(ptr + offset + 14)
                };
                return requestHead;
            }
        }
    }


    public class PackageInfo
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public string BodyJson { get; set; }
    }


    public enum ErrorCode : byte
    {
        Success = 0,
        NotSupoortServiceMethod = 1,
        SerializationDeserializationError = 2,
        ServiceException = 3,
        Timeout = 4,
        CmdIdError = 5,
    }
}
