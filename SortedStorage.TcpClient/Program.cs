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
            for (int i = 0; i < 100; i++)
            {
                string id = i.ToString();

                Task.Run(() =>
                {
                    var client = new AsynchronousClient();
                    client.StartClient();

                    DisplayResponse(client.Send(TcpRequest.Upsert("10", "novo 10")));
                    DisplayResponse(client.Send(TcpRequest.Get("10")));
                    DisplayResponse(client.Send(TcpRequest.Remove("10")));
                    DisplayResponse(client.Send(TcpRequest.Get("10")));

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