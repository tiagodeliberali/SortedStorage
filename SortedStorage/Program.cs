namespace SortedStorage;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.EventCounterCollector;

using SortedStorage.Adapter.Out;
using SortedStorage.Application;
using SortedStorage.TcpServer;

using System;
using System.Threading.Tasks;

class Program
{
    private static readonly EventCounterCollectionModule events = new EventCounterCollectionModule();

    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Should pass Path and InstrumentationKey as parameters");
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
        foreach (var item in TcpServiceEventSource.GetEvents())
        {
            events.Counters.Add(new EventCounterCollectionRequest(item.Key, item.Value));
        }

        foreach (var item in SortedStorageApplicationEventSource.GetEvents())
        {
            events.Counters.Add(new EventCounterCollectionRequest(item.Key, item.Value));
        }

        TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();

        configuration.InstrumentationKey = instrumentationKey;

        events.Initialize(configuration);
    }
}
