using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record Charities(ImmutableDictionary<string, Charity> Values) : IModel<Charities>
{
    public static IMetaModel<Charities> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<Charities>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override Charities Empty => new(ImmutableDictionary<string, Charity>.Empty);
        public override IEventProcessor<Charities> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
    }
    public static implicit operator Charities(ImmutableDictionary<string, Charity> values)
        => new(values);

    public bool Contains(string id)
        => Values.ContainsKey(id);

    private class Impl : EventProcessor<Charities>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override async ValueTask<Charities> NewCharity(Charities model, NewCharity e)
                => new(model.Values.Add(e.Code,
                    new Charity(e.Code, e.Name, BankInfo.Empty, null)));

            protected override async ValueTask<Charities> UpdateCharity(Charities model, UpdateCharity e)
            {
                var charity = model.Values[e.Code];
                var bankInfo = new BankInfo(e.Bank_name ?? charity.Bank.Name, e.Bank_account_no ?? charity.Bank.Account,
                    e.Bank_bic ?? charity.Bank.Bic);
                return new(model.Values.SetItem(charity.Id, charity with {Bank = bankInfo, Name = e.Name ?? charity.Name}));
            }

            protected override async ValueTask<Charities> CharityPartition(Charities model, CharityPartition e)
            {
                var charity = model.Values[e.Charity];
                return new(model.Values.SetItem(charity.Id,
                    charity with {Fractions = e.Partitions.ToImmutableDictionary(p => p.Holder, p => (Real)p.Fraction)}));
            }
        }
    }

}

public record Charity(string Id, string Name, BankInfo Bank, FractionSet? Fractions);

