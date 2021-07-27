using System;
using SortedStorage.TcpServer;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SortedStorage.TcpClient
{
    class Program
    {
        public static void Main(String[] args)
        {
            var r = new Random();
            var tasks = new List<Task>();

            for (int i = 0; i < 1000; i++)
            {
                string id = r.Next(0, 10000).ToString();

                tasks.Add(Task.Run(() =>
                {
                    var client = new AsynchronousClient();
                    client.StartClient();

                    DisplayResponse(client.Send(TcpRequest.Upsert(id, $"novo {id}")));
                    DisplayResponse(client.Send(TcpRequest.Get(id)));
                    // DisplayResponse(client.Send(TcpRequest.Remove(id)));
                    DisplayResponse(client.Send(TcpRequest.Get(id)));

                    client.Close();
                    Console.WriteLine($"[{nameof(Main)}] FINISHED CONSUMER {id}");
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void DisplayResponse(TcpResponse tcpResponse)
        {
            Console.WriteLine($"[{nameof(Main)}] Success: {tcpResponse.Success} with {tcpResponse.Entries.Count()} enties...");
            foreach (var item in tcpResponse.Entries)
            {
                Console.WriteLine($"[{nameof(Main)}] Key: {item.Key}, Value: {item.Value}");
            }
        }
    }
}