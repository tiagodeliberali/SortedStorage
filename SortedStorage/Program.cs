﻿using SortedStorage.Adapter.Out;
using SortedStorage.Application;
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
            FileManagerAdapter fileAdapter = new FileManagerAdapter(path);

            var storage = await StorageService.LoadFromFiles(fileAdapter);

            Console.WriteLine("LSM DB\n------");
            Console.WriteLine($"\nrunnung at {path}");

            while (true)
            {
                Console.WriteLine("[a (add), r (remove), g (get), m (merge sstable), t (transfer memtable), q (quit)]>");
                string action = Console.ReadLine();

                if (action.StartsWith("q"))
                {
                    break;
                }

                var data = action.Split(' ');

                if (action.StartsWith("a"))
                {
                    storage.Add(data[1], data[2]);
                }

                if (action.StartsWith("r"))
                {
                    storage.Remove(data[1]);
                }

                if (action.StartsWith("g"))
                {
                    string result = await storage.Get(data[1]);
                    Console.WriteLine(result ?? "no data found");
                }

                if (action.StartsWith("m"))
                {
                    await storage.MergeLastSSTables();
                }

                if (action.StartsWith("t"))
                {
                    await storage.TransferMemtableToSSTable();
                }
            }

            Console.WriteLine("bye!!");
        }
    }
}
