namespace FfAdmin.Calculator;

public record Donors(ImmutableDictionary<string, ImmutableList<string>> Values) : IModel<Donors>
{
    public static implicit operator Donors(ImmutableDictionary<string, ImmutableList<string>> dict)
        => new(dict);
    public static Donors Empty { get; } = new(ImmutableDictionary<string, ImmutableList<string>>.Empty);
    public static IEventProcessor<Donors> GetProcessor(IServiceProvider services) 
        => new Impl();

    public Donors Add(string donor, string donation)
        => Values.SetItem(donor, Values.GetValueOrDefault(donor, ImmutableList<string>.Empty).Add(donation));
    private class Impl : EventProcessor<Donors>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
            => new Calc(previousContext, currentContext);

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override Donors NewDonation(Donors model, NewDonation e)
                => model.Add(e.Donor, e.Donation);
        }
    }
}

