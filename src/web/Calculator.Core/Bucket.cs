namespace FfAdmin.Calculator;

public readonly record struct Bucket
{
    public int MaskBits { get; }
    private readonly int _index;

    public Bucket(int index, int maskBits)
    {
        MaskBits = maskBits;
        _index = index;
    }

    public int Index => _index & Mask;

    public bool Equals(Bucket other)
        => MaskBits == other.MaskBits && Index == other.Index;

    public override int GetHashCode()
        => HashCode.Combine(Index & Mask, MaskBits);

    public int Mask => (1 << MaskBits) - 1;

    public string Name
        => (Index & Mask).ToString("X").PadLeft((MaskBits - 1) / 4 + 1, '0');

    public static IEnumerable<Bucket> AllBuckets(int maskBits)
    {
        for (int i = 0; i < 1 << maskBits; i++)
            yield return new Bucket(i, maskBits);
    }
}