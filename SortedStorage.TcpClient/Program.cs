using System;
using SortedStorage.TcpServer;
using System.Linq;
using System.Threading.Tasks;

namespace SortedStorage.TcpClient
{
    class Program
    {
        public static void Main(String[] args)
        {
            var r = new Random();
            for (int i = 0; i < 100; i++)
            {
                string id = r.Next(0, 10000).ToString();

                Task.Run(() =>
                {
                    var client = new AsynchronousClient();
                    client.StartClient();

                    DisplayResponse(client.Send(TcpRequest.Upsert(id, $"novo {id}")));
                    DisplayResponse(client.Send(TcpRequest.Get(id)));
                    // DisplayResponse(client.Send(TcpRequest.Remove(id)));
                    DisplayResponse(client.Send(TcpRequest.Get(id)));

                    Console.WriteLine($"[{nameof(Main)}] FINISHED CONSUMER {id}");
                });
            }
            
            Console.ReadLine();
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