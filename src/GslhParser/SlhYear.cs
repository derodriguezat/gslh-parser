namespace GslhParser;

public sealed record SlhYear(string PathToSlhYear, int Year)
{
    private static readonly Dictionary<string, int> _months = new(){
    { "january", 1 },
    { "february", 2 },
    { "march", 3 },
    { "april", 4 },
    { "may", 5 },
    { "june", 6 },
    { "july", 7 },
    { "august", 8 },
    { "september", 9 },
    { "october", 10 },
    { "november", 11 },
    { "december", 12 }
};

    private List<SlhMonth>? _slhMonths = null;

    public IReadOnlyList<SlhMonth> GetSlhMonths()
    {
        if (this._slhMonths == null)
        {
            this._slhMonths = new List<SlhMonth>();
            foreach (var file in Directory.EnumerateFiles(this.PathToSlhYear))
            {
                var split = Path.GetFileNameWithoutExtension(file).Split('_');
                if (split.Length != 2 || !int.TryParse(split[0], out var year) || year != this.Year)
                {
                    continue;
                }

                if (!_months.TryGetValue(split[1].ToLower(), out var month))
                {
                    continue;
                }

                this._slhMonths.Add(new SlhMonth(file, month));
            }
        }

        return this._slhMonths!;
    }

    public bool TryGetMonth(int month, out SlhMonth? slhMonth)
    {
        slhMonth = this.GetSlhMonths().FirstOrDefault(m => m.Month == month);
        return slhMonth != null;
    }
}