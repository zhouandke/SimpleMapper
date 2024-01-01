using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
namespace SimpleRPC
{
    public class ServiceHost
    {
        private class MethodCallInfo
        {
            public Type ServiceType { get; set; }

            public ConstructorInfo ConstructorInfo { get; set; }

            public MethodInfo MethodInfo { get; set; }

            public Type ParameterType { get; set; }
        }


        private class RequestDealtResult
        {
            public RequestDealtResult(string serviceName, string methodName, byte code, string bodyJson)
            {
                ServiceName = serviceName;
                MethodName = methodName;
                Code = code;
                BodyJson = bodyJson;
            }

            public string ServiceName { get; }

            public string MethodName { get; }

            public byte Code { get; }

            public string BodyJson { get; }
        }


        private bool inited = false;
        /// <summary>
        /// 第一层key是 serviceName, 第二层key 是 methodName
        /// </summary>
        private ReadOnlyDictionary<string, ReadOnlyDictionary<string, MethodCallInfo>> serviceMap;
        private TcpListener tcpListener;

        public string Init(int port, params string[] dllPathes)
        {
            if (inited)
            {
                return "不能重复初始化";
            }

            var serviceMap = new Dictionary<string, ReadOnlyDictionary<string, MethodCallInfo>>();

            foreach (var dllPath in dllPathes)
            {
                var assembly = Assembly.LoadFrom(dllPath);
                foreach (var serviceType in assembly.GetTypes())
                {
                    var rpcServiceAttribute = serviceType.GetCustomAttribute<RpcServiceAttribute>();
                    if (rpcServiceAttribute == null)
                    {
                        continue;
                    }

                    var emptyConstructor = serviceType.GetConstructor(Array.Empty<Type>());
                    if (emptyConstructor == null)
                    {
                        Log.Warn($"{serviceType.Name} 没有空构造函数(暂时不想支持依赖注入), 忽略该服务类");
                        continue;
                    }

                    var methodMap = new Dictionary<string, MethodCallInfo>();
                    foreach (var method in serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        // 排除基本的方法
                        if (method.Name == nameof(object.GetType)
                            || method.Name == nameof(object.ToString)
                            || method.Name == nameof(object.Equals)
                            || method.Name == nameof(object.ReferenceEquals)
                            || method.Name == nameof(object.GetHashCode))
                        {
                            continue;
                        }

                        var parameters = method.GetParameters();
                        // 最多一个入参
                        if (parameters.Length > 1)
                        {
                            Log.Info($"{serviceType.Name}.{method.Name} 的参数超过1个, 忽略该方法");
                            continue;
                        }

                        var methodCallInfo = new MethodCallInfo
                        {
                            ServiceType = serviceType,
                            MethodInfo = method,
                            ConstructorInfo = emptyConstructor
                        };

                        if (parameters.Length == 1)
                        {
                            methodCallInfo.ParameterType = parameters[0].ParameterType;
                        }
                        else
                        {
                            methodCallInfo.ParameterType = null;
                        }

                        methodMap[method.Name] = methodCallInfo;
                    }

                    if (methodMap.Count == 0)
                    {
                        Log.Warn($"{serviceType.Name} 没有任何暴露方法, 忽略该服务类");
                        continue;
                    }

                    serviceMap[rpcServiceAttribute.ServiceName] = new ReadOnlyDictionary<string, MethodCallInfo>(methodMap);
                }
            }

            this.serviceMap = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, MethodCallInfo>>(serviceMap);

            try
            {
                var localEP = new IPEndPoint(IPAddress.Any, port);
                tcpListener = new TcpListener(localEP);
                tcpListener.Start(10);
            }
            catch (Exception ex)
            {
                Log.Error($"Socket服务启动失败: {ex.Message}");
                return $"Socket服务启动失败: {ex.Message}";
            }

            inited = true;

            Thread thread = new Thread(AcceptClientSocket);
            thread.Start();

            return null;
        }

        private void AcceptClientSocket()
        {
            try
            {
                while (true)
                {
                    var clientSocket = tcpListener.AcceptSocket();
                    Task.Run(() => WorkClientRequest(clientSocket));
                }

            }
            catch { }
        }



        private void WorkClientRequest(Socket socket)
        {
            socket.ReceiveTimeout = 500;
            var buffer = new byte[1024];
            int bufferAvailableSize = 0;

            PackageHead? requestHead = null;
            byte[] requestBodyBytes = null;
            int requestBodyBytesAvailableSize = 0;

            // 不处理任何 socket 相关的异常, 获取异常后线程退出
            try
            {
                while (true)
                {
                    int receivedSize;
                    SocketError errorCode;

                    if (requestHead != null)
                    {
                        receivedSize = socket.Receive(requestBodyBytes, requestBodyBytesAvailableSize, (int)requestHead.Value.BodySize - requestBodyBytesAvailableSize, SocketFlags.None, out errorCode);
                        requestBodyBytesAvailableSize += receivedSize;
                        if (receivedSize == 0)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        if (requestBodyBytesAvailableSize >= requestHead.Value.BodySize)
                        {
                            var dealRequestResult = DealRequest(requestBodyBytes);
                            SendResponse(requestHead.Value, dealRequestResult, socket);
                            requestHead = null;
                            requestBodyBytes = null;
                            requestBodyBytesAvailableSize = 0;
                        }
                        continue;
                    }

                    receivedSize = socket.Receive(buffer, bufferAvailableSize, buffer.Length - bufferAvailableSize, SocketFlags.None, out errorCode);
                    bufferAvailableSize += receivedSize;
                    if (receivedSize == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    while (bufferAvailableSize >= PackageHead.HeaderSize)
                    {
                        requestHead = Common.FindHead(buffer, bufferAvailableSize, out int scanedIndex);
                        if (requestHead == null)
                        {
                            Array.Copy(buffer, scanedIndex, buffer, 0, bufferAvailableSize - scanedIndex);
                            bufferAvailableSize -= scanedIndex;
                            continue;
                        }

                        requestBodyBytes = new byte[requestHead.Value.BodySize];
                        var shouldCopyToBodySize = Math.Min(requestBodyBytes.Length, bufferAvailableSize - scanedIndex - PackageHead.HeaderSize);
                        Array.Copy(buffer, scanedIndex + PackageHead.HeaderSize, requestBodyBytes, 0, shouldCopyToBodySize);
                        requestBodyBytesAvailableSize = shouldCopyToBodySize;

                        var bufferDealtSize = scanedIndex + PackageHead.HeaderSize + shouldCopyToBodySize;
                        Array.Copy(buffer, bufferDealtSize, buffer, 0, bufferAvailableSize - bufferDealtSize);
                        bufferAvailableSize -= bufferDealtSize;

                        if (requestBodyBytesAvailableSize >= requestHead.Value.BodySize)
                        {
                            var dealRequestResult = DealRequest(requestBodyBytes);
                            SendResponse(requestHead.Value, dealRequestResult, socket);
                            requestHead = null;
                            requestBodyBytes = null;
                            requestBodyBytesAvailableSize = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{socket.RemoteEndPoint} 出现异常, 线程推出: {ex.Message}");
            }
        }

        private RequestDealtResult DealRequest(byte[] bodyBytes)
        {
            PackageInfo requestPackageInfo;
            try
            {
                var json = Encoding.UTF8.GetString(bodyBytes);
                requestPackageInfo = JsonConvert.DeserializeObject<PackageInfo>(json);
            }
            catch (Exception ex)
            {
                return new RequestDealtResult("", "", (byte)ErrorCode.SerializationDeserializationError, $"解析 bodyByte 失败: {ex.Message}");
            }

            if (!serviceMap.TryGetValue(requestPackageInfo.ServiceName, out var methodMap))
            {
                return new RequestDealtResult(requestPackageInfo.ServiceName, requestPackageInfo.MethodName, (byte)ErrorCode.NotSupoortServiceMethod, $"不支持 {requestPackageInfo.ServiceName} 服务");
            }

            if (!methodMap.TryGetValue(requestPackageInfo.MethodName, out var methodCallInfo))
            {
                return new RequestDealtResult(requestPackageInfo.ServiceName, requestPackageInfo.MethodName, (byte)ErrorCode.NotSupoortServiceMethod, $"{requestPackageInfo.ServiceName} 服务不支持 {requestPackageInfo.MethodName} 方法");
            }

            object[] parameterObjectArray;
            if (methodCallInfo.ParameterType == null)
            {
                parameterObjectArray = Array.Empty<object>();
            }
            else
            {
                object parameterObject;
                try
                {
                    parameterObject = JsonConvert.DeserializeObject(requestPackageInfo.BodyJson, methodCallInfo.ParameterType);
                }
                catch (Exception ex)
                {
                    return new RequestDealtResult(requestPackageInfo.ServiceName, requestPackageInfo.MethodName, (byte)ErrorCode.SerializationDeserializationError, $"反序列化 {requestPackageInfo.ServiceName}.{requestPackageInfo.MethodName} 的参数错误: {ex.Message}");
                }
                parameterObjectArray = new[] { parameterObject };
            }

            object serviceInstance;
            try
            {
                serviceInstance = methodCallInfo.ConstructorInfo.Invoke(Array.Empty<object>());
            }
            catch (Exception ex)
            {
                return new RequestDealtResult(requestPackageInfo.ServiceName, requestPackageInfo.MethodName, (byte)ErrorCode.ServiceException, $"创建 {requestPackageInfo.ServiceName} 实例错误: {ex.Message}");
            }

            object returnValue;
            try
            {
                returnValue = methodCallInfo.MethodInfo.Invoke(serviceInstance, parameterObjectArray);
            }
            catch (Exception ex)
            {
                return new RequestDealtResult(requestPackageInfo.ServiceName, requestPackageInfo.MethodName, (byte)ErrorCode.ServiceException, $"{requestPackageInfo.ServiceName}.{requestPackageInfo.MethodName} 出现异常: {ex.Message}");
            }

            string responseBodyJson;
            try
            {
                responseBodyJson = JsonConvert.SerializeObject(returnValue);
            }
            catch (Exception ex)
            {
                return new RequestDealtResult(requestPackageInfo.ServiceName, requestPackageInfo.MethodName, (byte)ErrorCode.ServiceException, $"{requestPackageInfo.ServiceName}.{requestPackageInfo.MethodName} 的结果序列化异常: {ex.Message}");
            }

            return new RequestDealtResult(requestPackageInfo.ServiceName, requestPackageInfo.MethodName, 0, responseBodyJson);
        }

        private void SendResponse(PackageHead requestHead, RequestDealtResult requestDealtResult, Socket socket)
        {
            PackageInfo responsePackageInfo = new PackageInfo
            {
                ServiceName = requestDealtResult.ServiceName,
                MethodName = requestDealtResult.MethodName,
                BodyJson = requestDealtResult.BodyJson
            };
            var responseBodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responsePackageInfo));

            PackageHead responseHead = new PackageHead
            {
                HeadMark = PackageHead.HeadMarkValue,
                CmdId = requestHead.CmdId,
                BodySize = (uint)responseBodyBytes.Length,
                Type = requestHead.Type,
                ResponseCode = requestDealtResult.Code,
                TailMark = PackageHead.TailMarkValue,
            };

            var responseHeadBytes = responseHead.ToBytes();
            lock (socket)
            {
                int sendCount = 0;
                while (sendCount < responseHeadBytes.Length)
                {
                    sendCount += socket.Send(responseHeadBytes, sendCount, responseHeadBytes.Length - sendCount, SocketFlags.None);
                }

                sendCount = 0;
                while (sendCount < responseBodyBytes.Length)
                {
                    sendCount += socket.Send(responseBodyBytes, sendCount, responseBodyBytes.Length - sendCount, SocketFlags.None);
                }
            }
        }
    }
}
