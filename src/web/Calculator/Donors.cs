namespace FfAdmin.Calculator;

public record Donors(ImmutableDictionary<string, ImmutableList<string>> Values) : IModel<Donors>
{
    public static implicit operator Donors(ImmutableDictionary<string, ImmutableList<string>> dict)
        => new(dict);
    public static Donors Empty { get; } = new(ImmutableDictionary<string, ImmutableList<string>>.Empty);
    public static IEventProcessor<Donors> Processor { get; } = new Impl();

    public Donors Add(string donor, string donation)
        => Values.SetItem(donor, Values.GetValueOrDefault(donor, ImmutableList<string>.Empty).Add(donation));
    private class Impl : EventProcessor<Donors>
    {
        public override Donors Start => Empty;
        protected override Donors NewDonation(Donors model, IContext context, NewDonation e)
            => model.Add(e.Donor, e.Donation);
    }
}

