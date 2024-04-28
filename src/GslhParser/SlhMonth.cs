namespace GslhParser;

using System.Text.Json;

public sealed record SlhMonth(string PathToSlhMonth, int Month)
{
    private Dictionary<DateOnly, WorkVisit>? _workVisits = null;

    public IEnumerable<WorkVisit> GetWorkVisits()
    {
        if (this._workVisits == null)
        {
            this._workVisits = new Dictionary<DateOnly, WorkVisit>();
            {
                var jsonDocument = JsonDocument.Parse(File.ReadAllText(this.PathToSlhMonth));
                if (!jsonDocument.RootElement.TryGetProperty("timelineObjects", out var timelineElements))
                {
                    return Enumerable.Empty<WorkVisit>();
                }

                foreach (var item in timelineElements.EnumerateArray())
                {
                    if (!item.TryGetProperty("placeVisit", out var placeVisit))
                    {
                        continue;
                    }

                    if (!placeVisit.TryGetProperty("location", out var location))
                    {
                        continue;
                    }

                    if (!location.TryGetProperty("semanticType", out var semanticType) || semanticType.ToString() != "TYPE_WORK")
                    {
                        continue;
                    }

                    if (!placeVisit.TryGetProperty("duration", out var duration))
                    {
                        continue;
                    }

                    var startTimestamp = duration.GetProperty("startTimestamp").GetDateTime();
                    var endTimestamp = duration.GetProperty("endTimestamp").GetDateTime();
                    if (startTimestamp.DayOfWeek == DayOfWeek.Saturday || startTimestamp.DayOfWeek == DayOfWeek.Sunday)
                    {
                        continue;
                    }

                    if (!this._workVisits.TryGetValue(DateOnly.FromDateTime(startTimestamp), out var workVisit))
                    {
                        workVisit = new WorkVisit(DateOnly.FromDateTime(startTimestamp));
                        this._workVisits.Add(workVisit.Date, workVisit);
                        continue;
                    }

                    workVisit.TimeRanges.Add(new TimeRange(startTimestamp, endTimestamp));
                }
            }
        }

        return this._workVisits!.Values;
    }
}