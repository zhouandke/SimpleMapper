using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SimpleRPC
{
    public abstract class ClientBase
    {
        private class RequestCacheItem
        {
            public RequestCacheItem(uint cmdId)
            {
                CmdId = cmdId;
            }

            public uint CmdId { get; }



            public byte ResponseCode { get; set; }

            public string ResponseJson { get; set; }


            #region 适用同步调用
            public EventWaitHandle WaitHandle { get; set; }
            #endregion


            #region 适用异步调用
            public DateTime TimeoutTime { get; set; }

            public bool IsTaskExecuted { get; set; }

            public Task<Result<string>> Task { get; set; } 
            #endregion
        }


        private bool inited = false;
        protected readonly TcpClient tcpClient = new TcpClient();
        private readonly ConcurrentDictionary<uint, RequestCacheItem> requetCache = new ConcurrentDictionary<uint, RequestCacheItem>();
        private long currentCmdId = 0;


        public string Init(string remoteIP, int remotePort)
        {
            if (inited)
            {
                return "不能重复初始化";
            }

            try
            {
                var remoteEP = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
                tcpClient.Connect(remoteEP);
                tcpClient.Client.ReceiveTimeout = 500;

                inited = true;

                new Thread(ReceiveResponse).Start();
                new Thread(ClearTimeoutAsyncRequest).Start();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        private void ReceiveResponse()
        {
            var socket = tcpClient.Client;

            var buffer = new byte[1024];
            int bufferAvailableSize = 0;

            PackageHead? head = null;
            byte[] bodyBytes = null;
            int bodyBytesAvailableSize = 0;


            while (true)
            {
                int receivedSize;
                SocketError errorCode;

                if (head != null)
                {
                    receivedSize = socket.Receive(bodyBytes, bodyBytesAvailableSize, (int)head.Value.BodySize - bodyBytesAvailableSize, SocketFlags.None, out errorCode);
                    bodyBytesAvailableSize += receivedSize;
                    if (receivedSize == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    if (bodyBytesAvailableSize >= head.Value.BodySize)
                    {
                        DealResponse(head.Value, bodyBytes);
                        head = null;
                        bodyBytes = null;
                        bodyBytesAvailableSize = 0;
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
                    head = Common.FindHead(buffer, bufferAvailableSize, out int scanedIndex);
                    if (head == null)
                    {
                        Array.Copy(buffer, scanedIndex, buffer, 0, bufferAvailableSize - scanedIndex);
                        bufferAvailableSize -= scanedIndex;
                        continue;
                    }

                    bodyBytes = new byte[head.Value.BodySize];
                    var shouldCopyToBodySize = Math.Min(bodyBytes.Length, bufferAvailableSize - scanedIndex - PackageHead.HeaderSize);
                    Array.Copy(buffer, scanedIndex + PackageHead.HeaderSize, bodyBytes, 0, shouldCopyToBodySize);
                    bodyBytesAvailableSize = shouldCopyToBodySize;

                    var bufferDealtSize = scanedIndex + PackageHead.HeaderSize + shouldCopyToBodySize;
                    Array.Copy(buffer, bufferDealtSize, buffer, 0, bufferAvailableSize - bufferDealtSize);
                    bufferAvailableSize -= bufferDealtSize;

                    if (bodyBytesAvailableSize >= head.Value.BodySize)
                    {
                        DealResponse(head.Value, bodyBytes);
                        head = null;
                        bodyBytes = null;
                        bodyBytesAvailableSize = 0;
                    }
                }
            }
        }

        private void DealResponse(PackageHead responseHead, byte[] bodyBytes)
        {
            PackageInfo requestInfo;
            try
            {
                var json = Encoding.UTF8.GetString(bodyBytes);
                requestInfo = JsonConvert.DeserializeObject<PackageInfo>(json);
            }
            catch (Exception ex)
            {
                Log.Error($"{responseHead.CmdId} 反序列化bodyBytes错误: {ex.Message}{Environment.NewLine}{BitConverter.ToString(bodyBytes)}");
                return;
            }


            if (!requetCache.TryGetValue(responseHead.CmdId, out var requestCacheItem))
            {
                Log.Error($"requetCache 没有 CmdId= {responseHead.CmdId} 的 RequestCacheItem");
                return;
            }

            lock (requestCacheItem)
            {
                requestCacheItem.ResponseCode = responseHead.ResponseCode;
                requestCacheItem.ResponseJson = requestInfo.BodyJson;
            }

            if (requestCacheItem.Task == null)
            {
                requestCacheItem.WaitHandle.Set();
            }
            else
            {
                RemoveFromRequestCache(requestCacheItem);
                lock (requestCacheItem)
                {
                    if (!requestCacheItem.IsTaskExecuted)
                    {
                        requestCacheItem.IsTaskExecuted = true;
                        requestCacheItem.Task.Start();
                    }
                }
            }
        }



        protected Result<string> SendRequest(string serviceName, string methodName, object param, int timeoutInMillisecond = 2000)
        {
            var cmdId = (uint)Interlocked.Increment(ref currentCmdId);
            RequestCacheItem requestCacheItem = new RequestCacheItem(cmdId)
            {
                WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            };

            if (!requetCache.TryAdd(cmdId, requestCacheItem))
            {
                return (Result.Fail<string>((int)ErrorCode.CmdIdError, $"CmdId重复: {cmdId}"));
            }

            try
            {
                BuildRequestBytesThenSend(serviceName, methodName, param, cmdId);
            }
            catch (Exception ex)
            {
                RemoveFromRequestCache(requestCacheItem);
                return Result.Fail<string>((int)ErrorCode.SerializationDeserializationError, $"发送请求错误: {ex.Message}");
            }

            if (timeoutInMillisecond > 0)
            {
                requestCacheItem.WaitHandle.WaitOne(timeoutInMillisecond);
            }
            else
            {
                requestCacheItem.WaitHandle.WaitOne();
            }

            // 同步调用, 只会在调用线程 remove
            RemoveFromRequestCache(requestCacheItem);

            if (requestCacheItem.ResponseJson == null)
            {
                return Result.Fail<string>((int)ErrorCode.Timeout, "服务调用超时");
            }

            if (requestCacheItem.ResponseCode != 0)
            {
                return Result.Fail<string>(requestCacheItem.ResponseCode, requestCacheItem.ResponseJson);
            }

            return Result.Success(requestCacheItem.ResponseJson);
        }

        protected Task<Result<string>> SendRequestAsync(string serviceName, string methodName, object param, int timeoutInMillisecond = 2000)
        {
            var cmdId = (uint)Interlocked.Increment(ref currentCmdId);
            RequestCacheItem requestCacheItem = new RequestCacheItem(cmdId)
            {
                TimeoutTime = timeoutInMillisecond > 0 ? DateTime.Now.AddMilliseconds(timeoutInMillisecond) : DateTime.MaxValue
            };
            requestCacheItem.Task = new Task<Result<string>>(() =>
            {
                lock (requestCacheItem)
                {
                    if (requestCacheItem.ResponseJson == null)
                    {
                        return Result.Fail<string>((int)ErrorCode.Timeout, "服务调用超时");
                    }
                    return Result.Success(requestCacheItem.ResponseJson);
                }
            });

            if (!requetCache.TryAdd(cmdId, requestCacheItem))
            {
                requestCacheItem.Task.Dispose();
                return Task.FromResult(Result.Fail<string>((int)ErrorCode.CmdIdError, $"CmdId重复: {cmdId}"));
            }

            try
            {
                BuildRequestBytesThenSend(serviceName, methodName, param, cmdId);
            }
            catch (Exception ex)
            {
                RemoveFromRequestCache(requestCacheItem);
                return Task.FromResult(Result.Fail<string>((int)ErrorCode.SerializationDeserializationError, $"发送请求错误: {ex.Message}"));
            }

            return requestCacheItem.Task;
        }

        private void BuildRequestBytesThenSend(string serviceName, string methodName, object param, uint cmdId)
        {
            PackageInfo requestInfo = new PackageInfo
            {
                ServiceName = serviceName,
                MethodName = methodName,
                BodyJson = JsonConvert.SerializeObject(param)
            };
            var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestInfo));

            PackageHead requestHead = new PackageHead
            {
                HeadMark = PackageHead.HeadMarkValue,
                CmdId = cmdId,
                BodySize = (uint)bodyBytes.Length,
                Type = 0,
                ResponseCode = 0,
                TailMark = PackageHead.TailMarkValue,
            };

            var requestHeadBytes = requestHead.ToBytes();
            lock (tcpClient)
            {
                int sendCount = 0;
                while (sendCount < requestHeadBytes.Length)
                {
                    sendCount += tcpClient.Client.Send(requestHeadBytes, sendCount, requestHeadBytes.Length - sendCount, SocketFlags.None);
                }

                sendCount = 0;
                while (sendCount < bodyBytes.Length)
                {
                    sendCount += tcpClient.Client.Send(bodyBytes, sendCount, bodyBytes.Length - sendCount, SocketFlags.None);
                }
            }
        }



        private void RemoveFromRequestCache(RequestCacheItem requestCacheItem)
        {
            requetCache.TryRemove(requestCacheItem.CmdId, out requestCacheItem);
            requestCacheItem.WaitHandle?.Dispose();
        }

        private void ClearTimeoutAsyncRequest()
        {
            while (true)
            {
                var cmdIds = requetCache.Keys;
                var now = DateTime.Now;
                var threshold = now.AddSeconds(-1);  // 只清理1秒前超时的
                foreach (var cmdId in cmdIds)
                {
                    if (!requetCache.TryGetValue(cmdId, out var requestCacheItem))
                    {
                        continue;
                    }

                    if (requestCacheItem.Task == null)
                    {
                        continue;
                    }

                    if (requestCacheItem.TimeoutTime > threshold)
                    {
                        continue;
                    }

                    RemoveFromRequestCache(requestCacheItem);
                    lock (requestCacheItem)
                    {
                        if (!requestCacheItem.IsTaskExecuted)
                        {
                            requestCacheItem.IsTaskExecuted = true;
                            requestCacheItem.Task.Start();
                        }
                    }
                }

                var nextClearTime = now.Date.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second + 1);
                var sleepTimeSpan = nextClearTime - DateTime.Now;
                if (sleepTimeSpan > TimeSpan.Zero)
                {
                    Thread.Sleep(sleepTimeSpan);
                }
            }
        }
    }
}
