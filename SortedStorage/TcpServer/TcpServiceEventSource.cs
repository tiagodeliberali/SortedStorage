using System;
using System.Diagnostics.Tracing;

namespace SortedStorage.TcpServer
{
    [EventSource(Name = nameof(TcpServiceEventSource))]
    public class TcpServiceEventSource : EventSource
    {
        public static TcpServiceEventSource Log = new TcpServiceEventSource();

        private IncrementingEventCounter newClientsCounter;
        private IncrementingEventCounter readBytesCounter;
        private IncrementingEventCounter writeBytesCounter;

        public TcpServiceEventSource()
        {
            newClientsCounter = new IncrementingEventCounter(nameof(newClientsCounter), this)
            {
                DisplayName = "New clients",
                DisplayUnits = "un"
            };

            readBytesCounter = new IncrementingEventCounter(nameof(readBytesCounter), this)
            {
                DisplayName = "Read data",
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

        public void IncrementReadBytes(int bytes)
        {
            readBytesCounter.Increment(bytes);
        }

        public void IncrementSentBytes(int bytes)
        {
            writeBytesCounter.Increment(bytes);
        }
    }
}
