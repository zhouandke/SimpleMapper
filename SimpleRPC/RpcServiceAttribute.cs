using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRPC
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RpcServiceAttribute : Attribute
    {
        public RpcServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }

        public string ServiceName { get; }
    }
}
