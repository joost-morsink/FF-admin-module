using System.Collections.Immutable;
using FfAdmin.Calculator;

namespace FfAdmin.Calculator.Function;

public class CumulativeInterestCharting
{
    private List<Series> ConvertToChartingSeries(ImmutableList<OptionWorthRecord> history)
    {
        var series = new List<Series>();

        if (!history.Any()) return series;

        // Deduplicate by timestamp, taking the last record for each timestamp
        var deduplicatedHistory = history
            .GroupBy(r => r.Timestamp.Date)
            .Select(g => g.Last())
            .OrderBy(r => r.Timestamp)
            .ToList();

        if (!deduplicatedHistory.Any()) return series;

        var points = new List<DataPoint>();
        var minTime = deduplicatedHistory.Min(r => r.Timestamp);
        var maxTime = deduplicatedHistory.Max(r => r.Timestamp);
        var timeRange = (maxTime - minTime).TotalSeconds;

        for (int i = 0; i < deduplicatedHistory.Count; i++)
        {
            var record = deduplicatedHistory[i];
            var normalizedX = timeRange > 0 ? (record.Timestamp - minTime).TotalSeconds / timeRange : 0;
            points.Add(new DataPoint(normalizedX, (double)record.New.CumulativeInterest, record.Timestamp.DateTime));
        }

        series.Add(new Series("Cumulative Interest", "#5a9bd4", points));
        return series;
    }

    public string GenerateChartSvg(ImmutableList<OptionWorthRecord> history, string title = "Cumulative Interest Chart")
    {
        var chartSeries = ConvertToChartingSeries(history);
        var svgGenerator = new SvgGenerator();
        return svgGenerator.GenerateLineChartSvg(chartSeries, title);
    }
}
