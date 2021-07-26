using SortedStorage.Adapter.Out;
using SortedStorage.Application;
using SortedStorage.TcpServer;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SortedStorage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Console.WriteLine($"[{nameof(Main)}] runnung at {path}");

            FileManagerAdapter fileAdapter = new FileManagerAdapter(path);
            var storage = await StorageService.LoadFromFiles(fileAdapter);

            StorageListener tcpServer = new StorageListener(storage);

            var mergeTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20));
                    await storage.MergeLastSSTables();
                }
            });

            var transferTask = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20));
                    await storage.TransferMemtableToSSTable();
                }
            });

            tcpServer.StartListening();

            Console.WriteLine($"[{nameof(Main)}] bye!!");
        }
    }
}
