using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.EventCounterCollector;
using SortedStorage.Adapter.Out;
using SortedStorage.Application;
using SortedStorage.TcpServer;
using System;
using System.Threading.Tasks;

namespace SortedStorage
{
    class Program
    {
        private static EventCounterCollectionModule _events = new EventCounterCollectionModule();

        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Should pass path and InstrumentationKey as parameters");
            }

            SetupTelemetry(args[1]);

            string path = args[0];
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

        private static void SetupTelemetry(string instrumentationKey)
        {
            _events.Counters.Add(new EventCounterCollectionRequest(nameof(TcpServiceEventSource), "newClientsCounter"));
            _events.Counters.Add(new EventCounterCollectionRequest(nameof(TcpServiceEventSource), "readBytesCounter"));
            _events.Counters.Add(new EventCounterCollectionRequest(nameof(TcpServiceEventSource), "writeBytesCounter"));

            _events.Counters.Add(new EventCounterCollectionRequest(nameof(SortedStorageApplicationEventSource), "updateDurationCounter"));
            _events.Counters.Add(new EventCounterCollectionRequest(nameof(SortedStorageApplicationEventSource), "getDurationCounter"));


            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();

            configuration.InstrumentationKey = instrumentationKey;

            _events.Initialize(configuration);
        }
    }
}
