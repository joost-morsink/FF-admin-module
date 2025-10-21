using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FfAdmin.Calculator;
using FfAdmin.Common;

namespace FfAdmin.Calculator.Function;

public class OptionWorthHistoryCharting
{
    public List<Series> ConvertToChartingSeries(ImmutableList<OptionWorthRecord> history)
    {
        var series = new List<Series>();

        // Collect all records from all options
        var allRecords = history
            .OrderBy(record => record.Timestamp)
            .ToList();

        if (!allRecords.Any())
            return series;

        // Calculate normalized X values (0 to 1)
        var minTimestamp = allRecords.Min(r => r.Timestamp);
        var maxTimestamp = allRecords.Max(r => r.Timestamp);
        var timeRange = (maxTimestamp - minTimestamp).TotalSeconds;
        if (timeRange == 0) timeRange = 1; // Avoid division by zero

        // Process records sequentially to calculate cumulative donations from CONV_ENTER events and exits from CONV_EXIT events
        var processedRecords = new List<dynamic>();
        double cumulativeDonations = 0;
        double cumulativeExits = 0;

        var recordsByTimestamp = allRecords
            .GroupBy(record => record.Timestamp)
            .OrderBy(group => group.Key)
            .ToList();

        foreach (var group in recordsByTimestamp)
        {
            // Calculate cash increases from CONV_ENTER events
            var convEnterRecords = group.Where(r => r.EventType == EventType.CONV_ENTER).ToList();
            foreach (var record in convEnterRecords)
            {
                var cashIncrease = (double)record.New.Cash - (double)record.Old.Cash;
                if (cashIncrease > 0)
                {
                    cumulativeDonations += cashIncrease;
                }
            }

            // Calculate cash decreases from CONV_EXIT events
            var convExitRecords = group.Where(r => r.EventType == EventType.CONV_EXIT).ToList();
            foreach (var record in convExitRecords)
            {
                var cashDecrease = (double)record.Old.Cash - (double)record.New.Cash;
                if (cashDecrease > 0)
                {
                    cumulativeExits += cashDecrease;
                }
            }

            var timestamp = group.Key;
            var totalWorth = group.Select(r => (double)r.New.Cash + (double)r.New.Invested).Last();
            var totalWithUnentered = group.Select(r => (double)r.New.Cash + (double)r.New.Invested + (double)r.New.Unentered).Last();
            var totalWorthWithExits = totalWorth + cumulativeExits;

            processedRecords.Add(new
            {
                Timestamp = timestamp,
                TotalDonated = cumulativeDonations,
                TotalWorth = totalWorth,
                TotalWithUnentered = totalWithUnentered,
                TotalWorthWithExits = totalWorthWithExits,
                NormalizedX = timeRange > 0 ? (timestamp - minTimestamp).TotalSeconds / timeRange : 0
            });
        }

        // Group by approximately same X values and use the last Y value for each group
        var aggregatedRecords = processedRecords
            .GroupBy(item => Math.Round(item.NormalizedX, 6)) // Round to avoid floating point precision issues
            .Select(group => group.OrderBy(item => item.Timestamp).Last()) // Take the last (most recent) record for each X
            .OrderBy(item => item.Timestamp)
            .ToList();

        // Create data points for each series
        var totalDonatedPoints = new List<DataPoint>();
        var totalWorthPoints = new List<DataPoint>();
        var totalWithUnenteredPoints = new List<DataPoint>();
        var totalWorthWithExitsPoints = new List<DataPoint>();

        foreach (var item in aggregatedRecords)
        {
            var normalizedX = timeRange > 0 ? (item.Timestamp - minTimestamp).TotalSeconds / timeRange : 0;
            var timestamp = item.Timestamp.DateTime;

            totalDonatedPoints.Add(new DataPoint(normalizedX, item.TotalDonated, timestamp));
            totalWorthPoints.Add(new DataPoint(normalizedX, item.TotalWorth, timestamp));
            totalWithUnenteredPoints.Add(new DataPoint(normalizedX, item.TotalWithUnentered, timestamp));
            totalWorthWithExitsPoints.Add(new DataPoint(normalizedX, item.TotalWorthWithExits, timestamp));
        }

        // Create series with light colors
        series.Add(new Series("Total Donated", "#5a9bd4", totalDonatedPoints));        // Light Blue
        series.Add(new Series("Total Worth (Cash + Invested)", "#70c470", totalWorthPoints));    // Light Green
        series.Add(new Series("Total (Worth + Unentered)", "#d45a5a", totalWithUnenteredPoints)); // Light Red
        series.Add(new Series("Total Worth + Exits", "#9b9b9b", totalWorthWithExitsPoints)); // Light Grey

        return series;
    }

    public string GenerateChartSvg(ImmutableList<OptionWorthRecord> history, string title = "Option Worth History Chart")
    {
        var chartSeries = ConvertToChartingSeries(history);
        var svgGenerator = new SvgGenerator();
        return svgGenerator.GenerateLineChartSvg(chartSeries, title);
    }
}
