namespace SortedStorage.TcpServer;

using System.Collections.Generic;
using System.Diagnostics.Tracing;

[EventSource(Name = nameof(TcpServiceEventSource))]
public class TcpServiceEventSource : EventSource
{
    public static TcpServiceEventSource Log = new TcpServiceEventSource();

    private readonly IncrementingEventCounter newClientsCounter;
    private readonly IncrementingEventCounter receivedBytesCounter;
    private readonly IncrementingEventCounter writeBytesCounter;

    public TcpServiceEventSource()
    {
        newClientsCounter = new IncrementingEventCounter(nameof(newClientsCounter), this)
        {
            DisplayName = "New clients",
            DisplayUnits = "un"
        };

        receivedBytesCounter = new IncrementingEventCounter(nameof(receivedBytesCounter), this)
        {
            DisplayName = "Received data",
            DisplayUnits = "byte"
        };

        writeBytesCounter = new IncrementingEventCounter(nameof(writeBytesCounter), this)
        {
            DisplayName = "Sent data",
            DisplayUnits = "byte"
        };
    }

    public void IncrementActiveClients()
    {
        newClientsCounter.Increment();
    }

    public void IncrementReceivedBytes(int bytes)
    {
        receivedBytesCounter.Increment(bytes);
    }

    public void IncrementSentBytes(int bytes)
    {
        writeBytesCounter.Increment(bytes);
    }

    internal static IEnumerable<KeyValuePair<string, string>> GetEvents()
    {
        yield return KeyValuePair.Create(nameof(TcpServiceEventSource), nameof(newClientsCounter));
        yield return KeyValuePair.Create(nameof(TcpServiceEventSource), nameof(receivedBytesCounter));
        yield return KeyValuePair.Create(nameof(TcpServiceEventSource), nameof(writeBytesCounter));
    }
}
