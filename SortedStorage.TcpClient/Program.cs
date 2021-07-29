using SortedStorage.TcpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortedStorage.TcpClient
{
    class Program
    {
        public static void Main(String[] args)
        {
            var client = new AsynchronousClient();
            client.StartClient();

            while (true)
            {
                Console.WriteLine("[a (add) r (remove), g (get), gr (get in range), q (quit)]>");
                var action = Console.ReadLine().Split(' ');

                if (action[0] == "a")
                {
                    DisplayResponse(client.Send(TcpRequest.Upsert(action[1], action[2])));
                }

                if (action[0] == "r")
                {
                    DisplayResponse(client.Send(TcpRequest.Remove(action[1])));
                }

                if (action[0] == "g")
                {
                    DisplayResponse(client.Send(TcpRequest.Get(action[1])));
                }

                if (action[0] == "gr")
                {
                    DisplayResponse(client.Send(TcpRequest.GetInRange(action[1], action[2])));
                }

                if (action[0] == "q")
                {
                    client.Close();
                    break;
                }
            }

            Console.WriteLine("bye!!");
        }

        private static void DisplayResponse(TcpResponse tcpResponse)
        {
            Console.WriteLine($"[{nameof(Main)}] Success: {tcpResponse.Success} with {tcpResponse.Entries.Count()} enties...");
            foreach (var item in tcpResponse.Entries)
            {
                Console.WriteLine($"[{nameof(Main)}] Key: {item.Key}, Value: {item.Value}");
            }
        }

        private static void RunRandomTests()
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
    }
}