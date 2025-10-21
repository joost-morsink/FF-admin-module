namespace FfAdmin.Calculator.Function;

public class DonationRecordCharting
{
    public List<Series> ConvertToChartingSeries(IEnumerable<DonationRecord2> donationRecords)
    {
        var records = donationRecords.OrderBy(r => r.Timestamp).ToList();

        if (!records.Any())
            return new List<Series>();

        var series = new List<Series>();

        // Calculate normalized X values (0 to 1)
        var minTimestamp = records.Min(r => r.Timestamp);
        var maxTimestamp = records.Max(r => r.Timestamp);
        var timeRange = (maxTimestamp - minTimestamp).TotalSeconds;
        if (timeRange == 0) timeRange = 1; // Avoid division by zero

        // First pass: Calculate cumulative allocations for all records (including those that will be filtered out)
        var allocationsByTimestamp = new Dictionary<DateTimeOffset, double>();
        double runningAllocations = 0;
        foreach (var record in records)
        {
            if (record.Allocation != null)
            {
                runningAllocations += (double)record.Allocation.Amount;
            }
            allocationsByTimestamp[record.Timestamp] = runningAllocations;
        }

        // Calculate normalized X for each record and group by X value, keeping only the last record for each X
        var recordsWithX = records
            .Select(record => new { 
                Record = record, 
                NormalizedX = timeRange > 0 ? (record.Timestamp - minTimestamp).TotalSeconds / timeRange : 0 
            })
            .GroupBy(item => Math.Round(item.NormalizedX, 6)) // Round to avoid floating point precision issues
            .Select(group => group.OrderBy(item => item.Record.Timestamp).Last()) // Take the last (most recent) record for each X
            .OrderBy(item => item.Record.Timestamp)
            .ToList();

        // Create data points for each series using deduplicated records but with correct cumulative allocations
        var currentWorthPoints = new List<DataPoint>();
        var accumulatedAllocationsPoints = new List<DataPoint>();
        var sumPoints = new List<DataPoint>();

        foreach (var item in recordsWithX)
        {
            var record = item.Record;
            var normalizedX = item.NormalizedX;
            var timestamp = record.Timestamp.DateTime;
            var currentWorth = (double)record.Worth;
            var cumulativeAllocations = allocationsByTimestamp[record.Timestamp];

            var sum = currentWorth + cumulativeAllocations;

            // Add data points
            currentWorthPoints.Add(new DataPoint(normalizedX, currentWorth, timestamp));
            accumulatedAllocationsPoints.Add(new DataPoint(normalizedX, cumulativeAllocations, timestamp));
            sumPoints.Add(new DataPoint(normalizedX, sum, timestamp));
        }

        // Create series
        series.Add(new Series("Current Worth", "#2E86AB", currentWorthPoints));
        series.Add(new Series("Accumulated Allocations", "#A23B72", accumulatedAllocationsPoints));
        series.Add(new Series("Total (Worth + Allocations)", "#F18F01", sumPoints));

        return series;
    }

    public string GenerateChartSvg(IEnumerable<DonationRecord2> donationRecords, string title = "Donation Records Chart")
    {
        var series = ConvertToChartingSeries(donationRecords);
        var svgGenerator = new SvgGenerator();
        return svgGenerator.GenerateLineChartSvg(series, title);
    }
}
