namespace GslhParser;

public sealed record WorkVisit(DateOnly Date)
{
    public List<TimeRange> TimeRanges { get; } = new List<TimeRange>();
}