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
            tcpServer.StartListening();


            //    if (action.StartsWith("m"))
            //    {
            //        await storage.MergeLastSSTables();
            //    }

            //    if (action.StartsWith("t"))
            //    {
            //        await storage.TransferMemtableToSSTable();
            //    }

            Console.WriteLine($"[{nameof(Main)}] bye!!");
        }
    }
}
