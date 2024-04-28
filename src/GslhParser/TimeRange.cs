namespace GslhParser;

public sealed record TimeRange(DateTime Start, DateTime End)
{
    public TimeSpan Duration => this.End - this.Start;
}