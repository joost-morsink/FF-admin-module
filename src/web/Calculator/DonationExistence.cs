namespace FfAdmin.Calculator;

public class DonationExistence(IContext<Donations2, string, Donations2.Details> cDonations) : IModelCalculator<DonationExistence.Result, IEnumerable<string>>
{
    public async ValueTask<Result> Calculate(IContext context, IEnumerable<string> parameter)
    {
        var meta = Donations2.GetMetaModel();
        
        var header = await cDonations.GetValue(context);
        var result = new List<(bool, string)>();
        foreach (var (bucket, donations) in from d in parameter
                 let b = meta.GetBucket(header, d)
                 group d by b
                 into g
                 select (g.Key, Donations: g.AsEnumerable()))
        {
            var detail = await cDonations.GetValue(context, header, donations.First());
            foreach (var donation in donations)
                result.Add((detail.Values.ContainsKey(donation), donation));
        }

        var lookup = result.ToLookup(x => x.Item1, x => x.Item2);
        return new(lookup[true].ToImmutableList(), lookup[false].ToImmutableList());
    }

    public record Result(ImmutableList<string> Existing, ImmutableList<string> NotExisting);
}
