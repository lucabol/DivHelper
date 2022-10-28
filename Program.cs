﻿var merged = from sds in GetRecords<Sds>("sds.csv")
             join mstar in GetRecords<Mstar>("ms.csv") on sds.Ticker equals mstar.Ticker
             select new { rank = Math.Round(Rank(sds, mstar), 0,MidpointRounding.AwayFromZero), sds = sds, mstar = mstar};

var table = new ConsoleTable("Rank", "Tick", "Name", "Sect","M","A","U", "T", "Rtg", "Sfe", "Yld",
        "Str", "GrS", "1Gr", "5Gr", "20G");

foreach (var r in merged.OrderByDescending(m => m.rank).ThenByDescending(m => m.mstar.FwdYield))
    table.AddRow(r.rank, r.sds.Ticker,r.sds.Name,r.sds.Sector.Substring(0,4),r.mstar.Moat.t(),
        r.mstar.CapitalAllocation.t(),r.mstar.Uncertainty.t(), r.mstar.MoatTrend.t(), r.mstar.Rating.toStar(),r.sds.Safety,r.mstar.FwdYield,$"{r.sds.UninterruptedStreak,3}",$"{r.sds.GrowthStreak,3}",
        $"{Math.Round(r.sds.GrowthLatest,0),3}", $"{Math.Round(r.sds.Growth5Year,0),3}", $"{Math.Round(r.sds.Growth20Year??0,0),3}") ;

table.Write(Format.Minimal);

double Rank(Sds sds, Mstar mstar) =>
    (mstar.Moat == "Wide" ? 1 : 0) +
    (mstar.CapitalAllocation == "Exemplary" ? 1 : 0) +
    (sds.Safety >= 80 ? 1 : 0) +
    (sds.UninterruptedStreak >= 20 ? 0.5 : 0) +
    (sds.GrowthStreak >= 10 ? 0.5 : 0) +
    (sds.GrowthLatest >= 7 ? 0.2 : 0) +
    (sds.Growth5Year >= 7 ? 0.2 : 0) +
    (sds.Growth20Year >= 7 ? 0.2 : 0) +
    (mstar.Uncertainty == "Low" ? 0.5 : 0) +
    (mstar.Rating > 4.0 ? 0.5 : 0) +
    (mstar.MoatTrend == "Negative" ? -0.5 : 0);

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
    public static string toStar(this double rating) => (int)rating == 5 ? "*****" : (int)rating == 4 ? "****" : "***";
    public static string t(this string s) =>
        s == "Wide" || s == "Low" || s == "Positive" || s == "Exemplary" ? "+"
        : s == "Negative" ? "-" : "";
}