namespace FfAdmin.Calculator.Core;

public class HistoryCache<T> : IHistoryCache
    where T : class
{
    private int THRESHOLD = 5;
    private readonly Func<int, T> _creator;
    private ImmutableList<int> _positions;
    private ImmutableDictionary<int, T> _entries;

    public HistoryCache(Func<int, T> creator)
    {
        _creator = creator;
        _positions = ImmutableList<int>.Empty;
        _entries = ImmutableDictionary<int, T>.Empty;
    }

    public T GetAtPosition(int position)
    {
        if (_entries.TryGetValue(position, out var value))
            return value;

        var loc = GetLocation(position) - 1;
        Force(loc, position);
        return _entries[position];
    }

    object IHistoryCache.GetAtPosition(int position)
        => GetAtPosition(position);

    private void Force(int loc, int position)
    {
        if (loc < 0)
        {
            Calculate(0);
            Force(0, position);
        }
        else if (_positions[loc] != position)
        {
            var diff = position - _positions[loc];
            if (diff > THRESHOLD)
            {
                Force(loc, position - diff / 2);
                Cleanup(position - diff, position - diff / 2 - 1);
                Force(GetLocation(position) - 1, position);
            }
            else
                Calculate(position);
        }
    }

    private int GetLocation(int position)
    {
        var loc = _positions.BinarySearch(position);
        if (loc < 0)
            loc ^= -1;
        return loc;
    }

    private void Calculate(int pos)
        => Set(pos, _creator(pos));

    private void Set(int x, T value)
    {
        var loc = GetLocation(x);

        if (loc != _positions.Count && _positions[loc] == x)
            _entries = _entries.SetItem(x, value);
        else
        {
            _positions = _positions.Insert(loc, x);
            _entries = _entries.Add(x, value);
        }
    }

    public void Cleanup(int? minPosition = null, int? maxPosition = null)
    {
        if (_positions.Count == 0)
            return;

        var step = 1;
        var loc = maxPosition.HasValue
            ? GetLocation(maxPosition.Value) - 1
            : _positions.Count - 1;
        var cutoff = loc >= 0 ? _positions[loc] - step : -1;

        while (loc >= 0)
        {
            if (minPosition.HasValue && _positions[loc] < minPosition.Value)
                break;
            if (_positions[loc] >= cutoff)
            {
                _entries = _entries.Remove(_positions[loc]);
                _positions = _positions.RemoveAt(loc);
            }
            else
                cutoff -= step *= 2;

            loc--;
        }
    }
}
