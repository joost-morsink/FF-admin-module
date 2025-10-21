using System.Diagnostics;
using System.Globalization;
using System.Text;
// using FfAdmin.Calculator; // not required in this file

namespace FfAdmin.Calculator.Function;

public class SvgGenerator
{
    private const int Width = 1600;
    private const int Height = 900;
    // Separate horizontal and vertical margins so width/height margins can be different
    private const int MarginX = 120;
    private const int MarginY = 60;
    private const int LegendHeight = 80;
    private const int ChartWidth = Width - 2 * MarginX;
    private const int ChartHeight = Height - 2 * MarginY - LegendHeight;

    public string GenerateLineChartSvg(List<Series> series, string title = "Chart")
    {
        var svg = new StringBuilder();

        svg.AppendLine($"<svg width='{Width.ToString(CultureInfo.InvariantCulture)}' height='{Height.ToString(CultureInfo.InvariantCulture)}' xmlns='http://www.w3.org/2000/svg'>");
        svg.AppendLine("<style>");
        svg.AppendLine("text { font-family: Arial, sans-serif; font-size:10px; fill:#6c757d; }");
        svg.AppendLine(".title { font-size:24px; font-weight:bold; fill:#333; }");
        svg.AppendLine(".no-data { font-size:16px; fill:#6c757d; }");
        svg.AppendLine(".axis-label { font-size:18px; fill:#6c757d; }");
        svg.AppendLine(".legend-label { font-size:18px; fill:#333; }");
        svg.AppendLine(".last-value { font-size:18px; font-weight:bold; }");
        svg.AppendLine(".background { fill:#f7f7f7; stroke:none; }");
        svg.AppendLine(".chart-background { fill:white; stroke:none; }");
        svg.AppendLine(".grid-line { stroke:#e0e0e0; stroke-width:1; }");
        svg.AppendLine(".axis-line { stroke:#6c757d; stroke-width:2; }");
        svg.AppendLine(".data-line { stroke-width:3; fill:none; }");
        svg.AppendLine(".marker { stroke:white; stroke-width:1; }");
        svg.AppendLine(".connector { stroke-width:1; stroke-dasharray:2,2; }");
        svg.AppendLine("</style>");

        // Add light gray background for the entire SVG
        svg.AppendLine($"<rect x='0' y='0' width='{Width.ToString(CultureInfo.InvariantCulture)}' height='{Height.ToString(CultureInfo.InvariantCulture)}' class='background'/>");

        // Add title
        svg.AppendLine($"<text x='{(Width / 2).ToString(CultureInfo.InvariantCulture)}' y='{30.ToString(CultureInfo.InvariantCulture)}' text-anchor='middle' class='title'>{title}</text>");

        if (!series.Any() || !series.Any(s => s.Points.Any()))
        {
            svg.AppendLine("<text x='50' y='50' class='no-data'>No data available</text>");
        }
        else
        {
            GenerateLineChart(svg, series);
            GenerateLegend(svg, series);
        }

        svg.AppendLine("</svg>");

        return svg.ToString();
    }

    private void GenerateLineChart(StringBuilder svg, List<Series> series)
    {
        var allPoints = series.SelectMany(s => s.Points).ToList();
        if (!allPoints.Any()) return;

        var allValues = allPoints.Select(p => p.Y).ToList();
        var rawMax = allValues.Any() ? allValues.Max() : 1;
        var rawMin = allValues.Any() ? allValues.Min() : 0;
        rawMin = Math.Min(0, rawMin); // Include 0 as minimum when appropriate

        var rawRange = rawMax - rawMin;
        if (rawRange == 0) rawRange = Math.Abs(rawMax) > 0 ? Math.Abs(rawMax) : 1; // Avoid zero range

        // Determine "nice" tick spacing and rounded min/max
        const int desiredGridLines = 5;
        var niceRange = NiceNumber(rawRange, false);
        var tickSpacing = NiceNumber(niceRange / (desiredGridLines - 1), true);

        var niceMin = Math.Floor(rawMin / tickSpacing) * tickSpacing;
        var niceMax = Math.Ceiling(rawMax / tickSpacing) * tickSpacing;
        if (Math.Abs(niceMax - niceMin) < 1e-12)
        {
            // fallback if rounding collapsed the range
            niceMin = rawMin - 1;
            niceMax = rawMax + 1;
            tickSpacing = NiceNumber((niceMax - niceMin) / (desiredGridLines - 1), true);
        }

        // Draw a light gray background for the chart area (behind grid and plots)
        svg.AppendLine($"<rect x='{MarginX.ToString(CultureInfo.InvariantCulture)}' y='{MarginY.ToString(CultureInfo.InvariantCulture)}' width='{ChartWidth.ToString(CultureInfo.InvariantCulture)}' height='{ChartHeight.ToString(CultureInfo.InvariantCulture)}' class='chart-background'/>");

        // Generate grid lines using the nice range and spacing
        GenerateGridLines(svg, niceMin, niceMax, tickSpacing);

        // Generate X-axis date labels
        GenerateXAxisLabels(svg, allPoints);

        // Generate lines for each series using the nice range
        foreach (var seriesItem in series)
        {
            if (seriesItem.Points.Any())
            {
                GenerateLine(svg, seriesItem.Points, niceMax, niceMin, seriesItem.Color);
            }
        }

        // Draw last-value labels for each series
        GenerateLastValueLabels(svg, series, niceMax, niceMin, tickSpacing);

        // Y-axis
        svg.AppendLine($"<line x1='{MarginX.ToString(CultureInfo.InvariantCulture)}' y1='{MarginY.ToString(CultureInfo.InvariantCulture)}' x2='{MarginX.ToString(CultureInfo.InvariantCulture)}' y2='{(MarginY + ChartHeight).ToString(CultureInfo.InvariantCulture)}' class='axis-line'/>");

        // X-axis at y corresponding to value 0 (if 0 is within the nice range it will be on the chart)
        if (Math.Abs(niceMax - niceMin) > 1e-12)
        {
            // compute Y position using vertical margin
            var zeroY = MarginY + ChartHeight * (1 - (0 - niceMin) / (niceMax - niceMin));
            var zeroYStr = zeroY.ToString("F1", CultureInfo.InvariantCulture);
            var chartRight = (MarginX + ChartWidth).ToString(CultureInfo.InvariantCulture);
            svg.AppendLine($"<line x1='{MarginX.ToString(CultureInfo.InvariantCulture)}' y1='{zeroYStr}' x2='{chartRight}' y2='{zeroYStr}' class='axis-line'/>");
        }
    }

    private void GenerateXAxisLabels(StringBuilder svg, List<DataPoint> allPoints)
    {
        if (!allPoints.Any()) return;

        var minTime = allPoints.Min(dp => dp.Timestamp);
        var maxTime = allPoints.Max(dp => dp.Timestamp);
        if (minTime == maxTime)
        {
            // Single-point range: show that month
            var x = MarginX + ChartWidth / 2;
            var labelText = minTime.ToString("MMM yyyy");
            svg.AppendLine($"<text x='{x.ToString(CultureInfo.InvariantCulture)}' y='{(MarginY + ChartHeight + 20).ToString(CultureInfo.InvariantCulture)}' text-anchor='middle' class='axis-label'>{labelText}</text>");
            return;
        }

        const int desiredTicks = 5;
        var ticks = GetNiceDateTicks(minTime, maxTime, desiredTicks);
        if (ticks.Count == 0) return;

        var labelFormat = GetDateLabelFormat(ticks);
        var chartTop = MarginY.ToString(CultureInfo.InvariantCulture);
        var chartBottom = (MarginY + ChartHeight).ToString(CultureInfo.InvariantCulture);

        foreach (var tick in ticks)
        {
            // clamp tick into range
            if (tick < minTime) continue;
            if (tick > maxTime) continue;

            var tFrac = (tick - minTime).TotalSeconds / (maxTime - minTime).TotalSeconds;
            var x = MarginX + ChartWidth * tFrac;
            var xStr = x.ToString("F0", CultureInfo.InvariantCulture);

            // Vertical grid line
            svg.AppendLine("<line x1='" + xStr + "' y1='" + chartTop + "' x2='" + xStr + "' y2='" + chartBottom + "' class='grid-line'/>") ;
            // Label
            var labelText = tick.ToString(labelFormat, CultureInfo.InvariantCulture);
            svg.AppendLine($"<text x='{xStr}' y='{(MarginY + ChartHeight + 20).ToString(CultureInfo.InvariantCulture)}' text-anchor='middle' class='axis-label'>{labelText}</text>");
        }
    }

    // Returns a list of "nice" tick DateTime values between min and max (inclusive) aiming for ~desiredTicks ticks
    private static List<DateTime> GetNiceDateTicks(DateTime min, DateTime max, int desiredTicks)
    {
        var ticks = new List<DateTime>();
        if (min >= max) return ticks;

        var totalDays = (max - min).TotalDays;

        // Prefer years when range is several years
        if (totalDays >= 365 * 2)
        {
            var stepYears = GetYearStep(min, max, desiredTicks);

            // align to first multiple of stepYears at Jan 1
            var firstYear = (int)(Math.Floor(min.Year / (double)stepYears) * stepYears);
            if (firstYear < min.Year) firstYear += stepYears;
            var cur = new DateTime(firstYear, 1, 1);
            while (cur <= max)
            {
                if (cur >= min) ticks.Add(cur);
                cur = cur.AddYears(stepYears);
            }
            return ticks;
        }

        // Prefer months otherwise
        {
            var stepMonths = GetMonthStep(min, max, desiredTicks);

            // align to the first month boundary
            var startIndex = min.Year * 12 + (min.Month - 1);
            var firstIndex = (int)(Math.Ceiling(startIndex / (double)stepMonths) * stepMonths);
            var curYear = firstIndex / 12;
            var curMonth = (firstIndex % 12) + 1;
            var cur = new DateTime(curYear, curMonth, 1);
            while (cur <= max)
            {
                if (cur >= min) ticks.Add(cur);
                cur = cur.AddMonths(stepMonths);
            }

            return ticks;
        }
    }

    private static string GetDateLabelFormat(List<DateTime> ticks)
    {
        if (ticks.Count == 0) return "MMM yyyy";
        var span = (ticks.Last() - ticks.First()).TotalDays;
        if (span >= 365) return "yyyy";
        if (span >= 60) return "MMM yyyy";
        return "dd MMM";
    }

    private static int GetYearStep(DateTime min, DateTime max, int desiredTicks)
    {
        var approxYears = Math.Max(1.0, (max - min).TotalDays / 365.0);
        var divisor = Math.Max(1, desiredTicks - 1);
        var raw = approxYears / divisor;
        var nice = NiceNumber(raw, true);
        var rounded = (int)Math.Max(1, Math.Round(nice));
        return rounded;
    }

    private static int GetMonthStep(DateTime min, DateTime max, int desiredTicks)
    {
        var approxMonths = Math.Max(1.0, (max - min).TotalDays / 30.0);
        var divisor = Math.Max(1, desiredTicks - 1);
        var raw = NiceNumber(approxMonths / divisor, true);
        int[] allowed = { 1, 2, 3, 6, 12 };
        foreach (var a in allowed)
        {
            if (a >= raw) return a;
        }
        return 12;
    }

    private static int GetWeekStep(DateTime min, DateTime max, int desiredTicks)
    {
        var approxWeeks = (max - min).TotalDays / 7.0;
        var divisor = Math.Max(1, desiredTicks - 1);
        var raw = NiceNumber(approxWeeks / divisor, true);
        var step = (int)Math.Max(1, Math.Round(raw));
        return step;
    }

    private static int GetDayStep(DateTime min, DateTime max, int desiredTicks)
    {
        var approxDays = Math.Max(1.0, (max - min).TotalDays);
        var divisor = Math.Max(1, desiredTicks - 1);
        var raw = NiceNumber(approxDays / divisor, true);
        var step = (int)Math.Max(1, Math.Round(raw));
        return step;
    }

    private void GenerateGridLines(StringBuilder svg, double niceMin, double niceMax, double tickSpacing)
    {
        var marginStr = MarginX.ToString(CultureInfo.InvariantCulture);
        var chartRightStr = (MarginX + ChartWidth).ToString(CultureInfo.InvariantCulture);

        if (niceMax <= niceMin || tickSpacing <= 0)
        {
            // Fallback to a simple grid if inputs are invalid
            const int gridLines = 5;
            for (int i = 0; i <= gridLines; i++)
            {
                var y = MarginY + (ChartHeight * i / gridLines);
                var value = niceMax - ((niceMax - niceMin) * i / gridLines);

                var yStr = y.ToString("F1", CultureInfo.InvariantCulture);
                var yPlus4Str = (y + 4).ToString("F1", CultureInfo.InvariantCulture);
                var valueStr = FormatWithSuffix(value, "F1");

                // Horizontal grid line
                svg.AppendLine($"<line x1='{marginStr}' y1='{yStr}' x2='{chartRightStr}' y2='{yStr}' class='grid-line'/>\n");

                // Y-axis label
                svg.AppendLine($"<text x='{(MarginX - 10).ToString(CultureInfo.InvariantCulture)}' y='{yPlus4Str}' text-anchor='end' class='axis-label'>{valueStr}</text>");
            }

            return;
        }

        // Compute number of ticks based on spacing
        var tickCount = (int)Math.Round((niceMax - niceMin) / tickSpacing);
        if (tickCount <= 0) tickCount = 1;

        var format = GetLabelFormat(tickSpacing);

        for (int i = 0; i <= tickCount; i++)
        {
            var value = niceMax - i * tickSpacing;
            // Protect against floating point rounding issues by clamping
            if (value < niceMin - 1e-12) value = niceMin;

            // compute y using vertical margin
            var y = MarginY + ChartHeight * (1 - (value - niceMin) / (niceMax - niceMin));

            var yStr = y.ToString("F1", CultureInfo.InvariantCulture);
            var yPlus4Str = (y + 4).ToString("F1", CultureInfo.InvariantCulture);
            var labelStr = FormatWithSuffix(value, format);

            // Horizontal grid line
            svg.AppendLine("<line x1='" + marginStr + "' y1='" + yStr + "' x2='" + chartRightStr + "' y2='" + yStr + "' class='grid-line'/>\n");

            // Y-axis label (use horizontal margin for X offset)
            svg.AppendLine("<text x='" + (MarginX - 10).ToString(CultureInfo.InvariantCulture) + "' y='" + yPlus4Str + "' text-anchor='end' class='axis-label'>" + labelStr + "</text>");
        }
    }

    // Format large numbers using K (thousand), M (million), B (billion) suffixes.
    private static string FormatWithSuffix(double value, string numericFormat)
    {
        var abs = Math.Abs(value);
        string sign = value < 0 ? "-" : "";

        if (abs >= 1_000_000_000)
        {
            var scaled = abs / 1_000_000_000.0;
            var fmt = scaled < 10 ? "F1" : "F0";
            return sign + scaled.ToString(fmt, CultureInfo.InvariantCulture) + "B";
        }
        if (abs >= 1_000_000)
        {
            var scaled = abs / 1_000_000.0;
            var fmt = scaled < 10 ? "F1" : "F0";
            return sign + scaled.ToString(fmt, CultureInfo.InvariantCulture) + "M";
        }
        if (abs >= 1_000)
        {
            var scaled = abs / 1_000.0;
            var fmt = scaled < 10 ? "F1" : "F0";
            return sign + scaled.ToString(fmt, CultureInfo.InvariantCulture) + "K";
        }

        // For small numbers, use the provided numeric format
        return value.ToString(numericFormat, CultureInfo.InvariantCulture);
    }

    private void GenerateLine(StringBuilder svg, List<DataPoint> dataPoints, double maxValue, double minValue, string color)
    {
        if (!dataPoints.Any()) return;

        var pathData = new StringBuilder("M");
        var isFirstPoint = true;

        foreach (var dataPoint in dataPoints)
        {
            var x = MarginX + (ChartWidth * dataPoint.X); // X is already normalized (0 to 1)
            var y = MarginY + ChartHeight * (1 - (dataPoint.Y - minValue) / (maxValue - minValue));

            if (isFirstPoint)
            {
                pathData.Append($"{x.ToString("F1", CultureInfo.InvariantCulture)},{y.ToString("F1", CultureInfo.InvariantCulture)}");
                isFirstPoint = false;
            }
            else
            {
                pathData.Append($" L{x.ToString("F1", CultureInfo.InvariantCulture)},{y.ToString("F1", CultureInfo.InvariantCulture)}");
            }
        }

        svg.AppendLine($"<path d='{pathData}' class='data-line' stroke='{color}'/>");
    }

    // Helper: compute a "nice" number for ranges and tick spacing (Paul Heckbert algorithm)
    private static double NiceNumber(double range, bool round)
    {
        var exponent = Math.Floor(Math.Log10(range));
        var fraction = range / Math.Pow(10, exponent);
        double niceFraction;

        if (round)
        {
            if (fraction < 1.5)
                niceFraction = 1;
            else if (fraction < 3)
                niceFraction = 2;
            else if (fraction < 7)
                niceFraction = 5;
            else
                niceFraction = 10;
        }
        else
        {
            if (fraction <= 1)
                niceFraction = 1;
            else if (fraction <= 2)
                niceFraction = 2;
            else if (fraction <= 5)
                niceFraction = 5;
            else
                niceFraction = 10;
        }

        return niceFraction * Math.Pow(10, exponent);
    }

    private static string GetLabelFormat(double tickSpacing)
    {
        var abs = Math.Abs(tickSpacing);
        if (abs >= 1) return "F0";
        if (abs >= 0.1) return "F1";
        if (abs >= 0.01) return "F2";
        return "F3";
    }

    private void GenerateLegend(StringBuilder svg, List<Series> series)
    {
        var legendY = Height - LegendHeight + 20;
        var legendX = MarginX;

        foreach (var seriesItem in series)
        {
            svg.AppendLine($"<rect x='{legendX.ToString(CultureInfo.InvariantCulture)}' y='{legendY.ToString(CultureInfo.InvariantCulture)}' width='15' height='15' fill='{seriesItem.Color}'/>");
            svg.AppendLine($"<text x='{(legendX + 20).ToString(CultureInfo.InvariantCulture)}' y='{(legendY + 12).ToString(CultureInfo.InvariantCulture)}' class='legend-label'>{seriesItem.Name}</text>");
            legendX += 300;
        }
    }

    // Draw a small marker and a textual label for the last datapoint of each series
    private void GenerateLastValueLabels(StringBuilder svg, List<Series> series, double niceMax, double niceMin, double tickSpacing)
    {
        if (niceMax - niceMin == 0) return;

        // Use tickSpacing to pick an appropriate numeric format for the last-value labels
        const string labelFormat = "N2";

        // Collect label candidates (store markerY separately from label candidate Y)
        var candidates = new List<(Series SeriesItem, DataPoint Last, double MarkerX, double MarkerY, string LabelText)>();

        foreach (var s in series)
        {
            if (s.Points.Count == 0) continue;

            var last = s.Points.OrderBy(p => p.X).Last();

            var x = MarginX + (ChartWidth * last.X);
            var y = MarginY + ChartHeight * (1 - (last.Y - niceMin) / (niceMax - niceMin));

            var labelText = last.Y.ToString(labelFormat, CultureInfo.InvariantCulture);

            candidates.Add((s, last, x, y, labelText));
        }

        if (!candidates.Any()) return;

        // Sort candidates by their marker Y (top to bottom)
        candidates = candidates.OrderBy(c => c.MarkerY).ToList();

        // Prepare desired label positions (start at marker Y)
        var desiredYs = candidates.Select(c => c.MarkerY).ToList();

        // Spacing between stacked labels (in pixels)
        const double minSpacing = 18.0;
        var topLimit = MarginY; // don't place label above chart top
        var bottomLimit = MarginY + ChartHeight; // don't place label below chart bottom

        // Forward pass: ensure each label is at least minSpacing below the previous
        for (int i = 1; i < desiredYs.Count; i++)
        {
            if (desiredYs[i] < desiredYs[i - 1] + minSpacing)
            {
                desiredYs[i] = desiredYs[i - 1] + minSpacing;
            }
        }

        // Backward pass: if the last labels overflow bottom, try to push earlier labels up
        for (int i = desiredYs.Count - 1; i >= 0; i--)
        {
            if (desiredYs[i] > bottomLimit)
            {
                desiredYs[i] = bottomLimit;
                // push earlier ones up to preserve spacing
                for (int j = i - 1; j >= 0; j--)
                {
                    if (desiredYs[j] > desiredYs[j + 1] - minSpacing)
                    {
                        desiredYs[j] = desiredYs[j + 1] - minSpacing;
                    }
                }
            }
        }

        // Final clamp to top limit; if labels were pushed above top, move them down with spacing
        if (desiredYs.Any() && desiredYs[0] < topLimit)
        {
            desiredYs[0] = topLimit;
            for (int i = 1; i < desiredYs.Count; i++)
            {
                if (desiredYs[i] < desiredYs[i - 1] + minSpacing)
                    desiredYs[i] = desiredYs[i - 1] + minSpacing;
            }
        }

        // Now render markers and labels using adjusted Y positions
        for (int i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i];
            var markerX = c.MarkerX;
            var markerY = c.MarkerY; // marker stays at original data Y
            var labelY = desiredYs[i];
            var labelX = markerX + 12; // Extended further to the right

            // Draw marker at the actual last datapoint
            svg.AppendLine($"<circle cx='{markerX.ToString(InventoryInvariant(), CultureInfo.InvariantCulture)}' cy='{markerY.ToString(InventoryInvariant(), CultureInfo.InvariantCulture)}' r='3.5' class='marker' fill='{c.SeriesItem.Color}'/>");

            // Always draw a connector line from marker to label
            var lineEndX = labelX - 2; // Stop just before the text starts
            svg.AppendLine($"<line x1='{markerX.ToString(InventoryInvariant(), CultureInfo.InvariantCulture)}' y1='{markerY.ToString("F1", CultureInfo.InvariantCulture)}' x2='{lineEndX.ToString("F1", CultureInfo.InvariantCulture)}' y2='{labelY.ToString("F1", CultureInfo.InvariantCulture)}' class='connector' stroke='{c.SeriesItem.Color}' />");

            // Draw the text label
            svg.AppendLine($"<text x='{labelX.ToString(InventoryInvariant(), CultureInfo.InvariantCulture)}' y='{(labelY + 4).ToString(InventoryInvariant(), CultureInfo.InvariantCulture)}' text-anchor='start' class='last-value' fill='{c.SeriesItem.Color}'>" + System.Security.SecurityElement.Escape(c.LabelText) + "</text>");
        }
    }

    // Helper to get InvariantCulture format string for ToString calls (avoids repeating formatting strings inline)
    private static string InventoryInvariant() => "F1";
}

[DebuggerDisplay("({X},{Y}) @ {Timestamp}")]
public class DataPoint
{
    public double X { get; set; } // Normalized X (0-1)
    public double Y { get; set; } // Value
    public DateTime Timestamp { get; set; } // For X-axis labeling

    public DataPoint(double x, double y, DateTime timestamp)
    {
        X = x;
        Y = y;
        Timestamp = timestamp;
    }
}

public class Series
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public List<DataPoint> Points { get; set; } = new();

    public Series(string name, string color, List<DataPoint>? points = null)
    {
        Name = name;
        Color = color;
        Points = points ?? new List<DataPoint>();
    }
}
