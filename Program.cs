var merged = from sds in GetRecords<Sds>("sds.csv")
             join mstar in GetRecords<Mstar>("ms.csv") on sds.Ticker equals mstar.Ticker
             select new { primaryRank = PrimaryRank(sds, mstar), secondaryRank = SecondaryRank(sds, mstar), sds, mstar};

var table = new ConsoleTable("PRnk", "SRnk", "Tick", "Name", "Sect","M","A","U", "T", "Rtg", "Sfe", "Yld", "TYld",
        "Str", "GrS", "1Gr", "5Gr", "20G");

foreach (var r in merged.OrderByDescending(m => m.primaryRank).ThenByDescending(m => m.secondaryRank).ThenByDescending(m => m.mstar.FwdYield))
    table.AddRow(r.primaryRank, r.secondaryRank, r.sds.Ticker, r.sds.Name.ToName(),r.sds.Sector[..4],r.mstar.Moat.T(),
        r.mstar.CapitalAllocation.T(),r.mstar.Uncertainty.T(), r.mstar.MoatTrend.T(),
        r.mstar.Rating.ToStar(),r.sds.Safety,$"{Math.Round(r.mstar.FwdYield,2),3}",$"{Math.Round(r.sds.Yield,2),3}", $"{r.sds.UninterruptedStreak,3}",$"{r.sds.GrowthStreak,3}",
        $"{Math.Round(r.sds.GrowthLatest,0),3}", $"{Math.Round(r.sds.Growth5Year,0),3}", $"{Math.Round(r.sds.Growth20Year??0,0),3}") ;


table.Write(Format.Minimal);

Console.ReadLine();

var leftOuter = from sds in GetRecords<Sds>("sds.csv")
                join mstar in GetRecords<Mstar>("ms.csv")
                on sds.Ticker equals mstar.Ticker
                into left
                from m in left.DefaultIfEmpty()
                select new { sds, m};

ConsoleTable.From(leftOuter
    .Where(k => k.m == null)
    .Select(k => k.sds)
    .Where(k => k.Safety >= 80)
    .OrderByDescending(k => k.Yield))
    .Write(Format.Minimal);

Console.ReadLine();

var leftOuterMs =
                from mstar in GetRecords<Mstar>("ms.csv")
                join sds in GetRecords<Sds>("sds.csv")
                on mstar.Ticker equals sds.Ticker
                into left
                from s in left.DefaultIfEmpty()
                select new { mstar, s};

ConsoleTable.From(leftOuterMs
    .Where(k => k.s == null)
    .Select(k => k.mstar)
    .Where(k => k.Moat == "Wide")
    .OrderByDescending(k => k.FwdYield))
    .Write(Format.Minimal);

int PrimaryRank(Sds sds, Mstar mstar) =>
    (mstar.Moat == "Wide" ? 1 : 0) +
    (sds.Safety >= 80 ? 1 : 0)
    ;
int SecondaryRank(Sds sds, Mstar mstar) =>
    (mstar.CapitalAllocation == "Exemplary" ? 4 : 0) +
    (mstar.Rating > 4.0 ? 4 : 0) +
    (sds.UninterruptedStreak >= 20 ? 1 : 0) +
    (sds.GrowthStreak >= 10 ? 1 : 0) +
    (sds.GrowthLatest >= 7 ? 1 : 0) +
    (sds.Growth5Year >= 7 ? 1 : 0) +
    (sds.Growth20Year >= 7 ? 1 : 0) +
    (mstar.Uncertainty == "Low" ? 2 : 0) +
    (mstar.MoatTrend == "Negative" ? -2 : 0)
    ;

IEnumerable<T> GetRecords<T>(string file) {
    using var reader = new StreamReader(file);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    return csv.GetRecords<T>().ToArray();
}

public record Sds
{
    [Name("Ticker")] public string Ticker { get; set; } = default!;
    [Name("Name")] public string Name { get; set; } = default!;
    [Name("Sector")] public string Sector { get; set; } = default!;
    [Name("Dividend Yield")] public double Yield { get; set; } = default!;
    [Name("Dividend Safety")] public int Safety { get; set; } = default!;
    [Name("Dividend Growth (Latest)")] public double GrowthLatest { get; set; } = default!;
    [Name("5-Year Dividend Growth")] public double Growth5Year { get; set; } = default!;
    [Name("20-Year Dividend Growth")] public double? Growth20Year { get; set; } = default!;
    [Name("Dividend Growth Streak (Years)")] public int GrowthStreak { get; set; } = default!;
    [Name("Uninterrupted Dividend Streak (Years)")] public int UninterruptedStreak { get; set; } = default!;
}

public record Mstar
{
    [Name("Ticker ")] public string Ticker { get; set; } = default!;
    [Name("Dividend Yield (Forward) (%)")] public double FwdYield { get; set; } = default!;
    [Name("Dividend Yield (Trailing) (%)")] public double TrailYield { get; set; } = default!;
    [Name("Economic Moat ")] public string Moat { get; set; } = default!;
    [Name("Economic Moat Trend ")] public string MoatTrend { get; set; } = default!;
    [Name("Capital Allocation ")] public string CapitalAllocation { get; set; } = default!;
    [Name("Morningstar Rating for Stocks ")] public double Rating { get; set; } = default!;
    [Name("Fair Value Uncertainty ")] public string Uncertainty { get; set; } = default!;
}

public static class Stex {
    public static string ToName(this string s) => s.Length > 25 ? s[..25] : s;
    public static string ToStar(this double rating) => (int)rating == 5 ? "*****" : (int)rating == 4 ? "****" : "***";
    public static string T(this string s) =>
        s == "Wide" || s == "Low" || s == "Positive" || s == "Exemplary" ? "+"
        : s == "Negative" ? "-" : "";
}
