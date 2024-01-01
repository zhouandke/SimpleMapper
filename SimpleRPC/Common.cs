using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRPC
{
    internal class Common
    {
        public static unsafe PackageHead? FindHead(byte[] buffer, int bufferAvailableSize, out int scanedIndex)
        {
            fixed (byte* ptr = buffer)
            {
                for (scanedIndex = 0; scanedIndex <= bufferAvailableSize - PackageHead.HeaderSize; scanedIndex++)
                {
                    if (*(uint*)(ptr + scanedIndex) != PackageHead.HeadMarkValue)
                    {
                        continue;
                    }
                    if (*(ushort*)(ptr + scanedIndex + 14) != PackageHead.TailMarkValue)
                    {
                        continue;
                    }
                    return PackageHead.Convert(buffer, scanedIndex);
                }
            }

            return null;
        }
    }
}
