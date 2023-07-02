namespace SortedStorage.Application;

using System.Collections.Generic;
using System.Diagnostics.Tracing;

[EventSource(Name = nameof(SortedStorageApplicationEventSource))]
public class SortedStorageApplicationEventSource : EventSource
{
    public static SortedStorageApplicationEventSource Log = new SortedStorageApplicationEventSource();

    private EventCounter updateDurationCounter;
    private EventCounter getDurationCounter;

    public SortedStorageApplicationEventSource()
    {
        updateDurationCounter = new EventCounter(nameof(updateDurationCounter), this)
        {
            DisplayName = "Update Processing Time",
            DisplayUnits = "ticks"
        };

        getDurationCounter = new EventCounter(nameof(getDurationCounter), this)
        {
            DisplayName = "Get Processing Time",
            DisplayUnits = "ticks"
        };
    }

    public void ReportUpdateDurationInMs(double ticks)
    {
        updateDurationCounter.WriteMetric(ticks);
    }

    public void ReportGetDurationInMs(double ticks)
    {
        getDurationCounter.WriteMetric(ticks);
    }

    public static IEnumerable<KeyValuePair<string, string>> GetEvents()
    {
        yield return KeyValuePair.Create(nameof(SortedStorageApplicationEventSource), nameof(updateDurationCounter));
        yield return KeyValuePair.Create(nameof(SortedStorageApplicationEventSource), nameof(getDurationCounter));
    }
}
