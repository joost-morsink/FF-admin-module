using System.Collections.Immutable;

namespace FfAdmin.ModelCache.Abstractions;

public record HashesForBranch(ImmutableDictionary<int, byte[]> Hashes)
{
    public static HashesForBranch Empty { get; } = new(ImmutableDictionary<int, byte[]>.Empty);
}
