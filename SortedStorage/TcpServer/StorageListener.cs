using SortedStorage.Application;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SortedStorage.TcpServer
{
    public class StorageListener : SocketListener
    {
        private readonly StorageService storage;

        public StorageListener(StorageService storage)
        {
            this.storage = storage;
        }

        protected override async Task<TcpResponse> ProcessRequest(TcpRequest tcpRequest)
        {
            try
            {
                switch (tcpRequest.Type)
                {
                    case RequestType.Upsert:
                        storage.Add(tcpRequest.Key, tcpRequest.Value);
                        return TcpResponse.SuccessResult();

                    case RequestType.Remove:
                        storage.Remove(tcpRequest.Key);
                        return TcpResponse.SuccessResult();

                    case RequestType.Get:
                        string result = await storage.Get(tcpRequest.Key);
                        return result == null
                            ? TcpResponse.SuccessResult()
                            : TcpResponse.SuccessGet(new List<KeyValueEntry> { new KeyValueEntry(tcpRequest.Key, result) });

                    case RequestType.GetInRange:
                        var list = new List<KeyValueEntry>();
                        await foreach(var item in storage.GetInRange(tcpRequest.Key, tcpRequest.Value))
                        {
                            list.Add(new KeyValueEntry(item.Key, item.Value));
                        }
                        return TcpResponse.SuccessGet(list);

                    default:
                        return TcpResponse.FailureResult();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(StorageListener)}] {e}");
                return TcpResponse.FailureResult();
            }
        }
    }
}
