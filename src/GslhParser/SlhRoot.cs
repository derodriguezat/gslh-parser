namespace GslhParser;

public sealed record SlhRoot(string PathToSlhRoot)
{
    private List<SlhYear>? _slhYears = null;

    public bool TryGetYear(int year, out SlhYear? slhYear)
    {
        slhYear = this.GetSlhYears().FirstOrDefault(y => y.Year == year);
        return slhYear != null;
    }

    public IReadOnlyList<SlhYear> GetSlhYears()
    {
        if (this._slhYears == null)
        {
            this._slhYears = new List<SlhYear>();
            foreach (var directory in Directory.EnumerateDirectories(this.PathToSlhRoot))
            {
                if (!int.TryParse(Path.GetFileName(directory), out var year))
                {
                    continue;
                }

                this._slhYears.Add(new SlhYear(directory, year));
            }
        }

        return this._slhYears!;
    }
}