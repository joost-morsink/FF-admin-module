using System;
using System.Collections.Immutable;
using System.Linq;

namespace FfAdmin.Common;

public struct HashValue : IEquatable<HashValue>
{
    private readonly ImmutableArray<byte> _bytes;

    private HashValue(ImmutableArray<byte> bytes)
    {
        _bytes = bytes;
    }
    public static implicit operator HashValue(byte[] bytes)
        => new (bytes.ToImmutableArray());

    public static implicit operator HashValue(ImmutableArray<byte> bytes)
        => new (bytes);
    public static implicit operator HashValue(ReadOnlySpan<byte> bytes)
        => new (bytes.ToImmutableArray());

    public static implicit operator ReadOnlySpan<byte>(HashValue value)
        => value.AsSpan();

    public ReadOnlySpan<byte> AsSpan()
        => _bytes.AsSpan();
    
    public bool Equals(HashValue other)
        => _bytes.SequenceEqual(other._bytes);

    public override bool Equals(object? obj)
        => obj is HashValue other && Equals(other);

    public override int GetHashCode()
        => BitConverter.ToInt32(this.AsSpan());
    
    public static bool operator ==(HashValue left, HashValue right)
        => left.Equals(right);
    
    public static bool operator !=(HashValue left, HashValue right)
        => !left.Equals(right);
    
    public static bool Equals(HashValue left, HashValue right)
        => left == right;
    
}
