using System.Collections.Generic;
using System.Linq;
using FfAdmin.Calculator;

namespace FfAdmin.Calculator.Function;

public class DonorDashboardStatsCharting
{
    public List<Series> ConvertToChartingSeries(DonorDashboardStats2.Stat stats)
    {
        // Collect all donation records from all donations
        var allRecords = stats.Donations.Values
            .SelectMany(stat => stat.Records.Select(record => new { Record = record, DonationId = stat.Donation.Id, OriginalDonationAmount = (double)stat.Donation.Amount }))
            .OrderBy(item => item.Record.Timestamp)
            .ToList();

        // Track donations that have been added to cumulative donations
        var donationsAddedToCumulative = new HashSet<string>();

        // Process records sequentially to generate aggregated collection
        var aggregatedRecords = new List<AggregatedRecord>();
        var currentWorthByDonation = new Dictionary<string, double>();
        double cumulativeAllocations = 0;
        double cumulativeDonations = 0;

        foreach (var item in allRecords)
        {
            var record = item.Record;
            var donationId = item.DonationId;

            // Update the current worth for this donation
            currentWorthByDonation[donationId] = (double)record.Worth;

            // Add any allocation to the cumulative total
            if (record.Allocation != null)
            {
                cumulativeAllocations += (double)record.Allocation.Amount;
            }

            // Add to cumulative donations when we first encounter this donation
            if (!donationsAddedToCumulative.Contains(donationId))
            {
                cumulativeDonations += item.OriginalDonationAmount;
                donationsAddedToCumulative.Add(donationId);
            }

            // Calculate total worth across all donations at this point in time
            var totalWorth = currentWorthByDonation.Values.Sum();

            // Create aggregated record
            aggregatedRecords.Add(new AggregatedRecord(
                record.Timestamp,
                totalWorth,
                cumulativeAllocations,
                cumulativeDonations
            ));
        }

        if (!aggregatedRecords.Any())
            return new List<Series>();

        var series = new List<Series>();

        // Calculate normalized X values (0 to 1)
        var minTimestamp = aggregatedRecords.Min(r => r.Timestamp);
        var maxTimestamp = aggregatedRecords.Max(r => r.Timestamp);
        var timeRange = (maxTimestamp - minTimestamp).TotalSeconds;
        if (timeRange == 0) timeRange = 1; // Avoid division by zero

        // Calculate normalized X for each record and group by X value, keeping only the last record for each X
        var recordsByTimestamp = aggregatedRecords
            .Select(record => new
            {
                Timestamp = record.Timestamp,
                TotalWorth = record.TotalWorth,
                CumulativeAllocations = record.CumulativeAllocations,
                CumulativeDonations = record.CumulativeDonations,
                NormalizedX = timeRange > 0 ? (record.Timestamp - minTimestamp).TotalSeconds / timeRange : 0
            })
            .ToList();

        var recordsWithX = recordsByTimestamp
            .GroupBy(item => Math.Round(item.NormalizedX, 6)) // Round to avoid floating point precision issues
            .Select(group => group.OrderBy(item => item.Timestamp).Last()) // Take the last (most recent) record for each X
            .OrderBy(item => item.Timestamp)
            .ToList();

        // Create data points for each series using deduplicated records
        var currentWorthPoints = new List<DataPoint>();
        var accumulatedAllocationsPoints = new List<DataPoint>();
        var cumulativeDonationsPoints = new List<DataPoint>();
        var sumPoints = new List<DataPoint>();

        foreach (var item in recordsWithX)
        {
            var normalizedX = item.NormalizedX;
            var timestamp = item.Timestamp.DateTime;
            var currentWorth = item.TotalWorth;
            var allocationsValue = item.CumulativeAllocations;
            var donationsValue = item.CumulativeDonations;

            var sum = currentWorth + allocationsValue;

            // Add data points
            currentWorthPoints.Add(new DataPoint(normalizedX, currentWorth, timestamp));
            accumulatedAllocationsPoints.Add(new DataPoint(normalizedX, allocationsValue, timestamp));
            cumulativeDonationsPoints.Add(new DataPoint(normalizedX, donationsValue, timestamp));
            sumPoints.Add(new DataPoint(normalizedX, sum, timestamp));
        }

        // Create series
        series.Add(new Series("Current Worth", "#5a9bd4", currentWorthPoints));        // Light Blue
        series.Add(new Series("Accumulated Allocations", "#9b9b9b", accumulatedAllocationsPoints));  // Light Grey
        series.Add(new Series("Cumulative Donations", "#70c470", cumulativeDonationsPoints));        // Light Green
        series.Add(new Series("Total (Worth + Allocations)", "#d45a5a", sumPoints));  // More Saturated Light Red

        return series;
    }

    public string GenerateChartSvg(DonorDashboardStats2.Stat stats, string title = "Donor Dashboard Chart")
    {
        var chartSeries = ConvertToChartingSeries(stats);
        var svgGenerator = new SvgGenerator();
        return svgGenerator.GenerateLineChartSvg(chartSeries, title);
    }

    private record AggregatedRecord(DateTimeOffset Timestamp, double TotalWorth, double CumulativeAllocations, double CumulativeDonations);
}
