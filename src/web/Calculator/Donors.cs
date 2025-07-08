
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record Donors(ImmutableDictionary<string, ImmutableList<string>> Values) : IModel<Donors>
{
    public static IMetaModel<Donors> GetMetaModel()
        => Meta.Instance;

    private class Meta : IModel<Donors>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override Donors Empty => new(ImmutableDictionary<string, ImmutableList<string>>.Empty);
        public override IEventProcessor<Donors> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
    }
    public static implicit operator Donors(ImmutableDictionary<string, ImmutableList<string>> dict)
        => new(dict);

    public Donors Add(string donor, string donation)
        => Values.SetItem(donor, Values.GetValueOrDefault(donor, ImmutableList<string>.Empty).Add(donation));
    private class Impl : EventProcessor<Donors>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
            => new Calc(previousContext, currentContext);

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override async ValueTask<Donors> NewDonation(Donors model, NewDonation e)
                => model.Add(e.Donor, e.Donation);
        }
    }
}

