using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record Allocations(ImmutableList<Allocation2> Values) : IModel<Allocations>
{
    public static IMetaModel<Allocations> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<Allocations>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override Allocations Empty { get; } = new([]);
        public override IEventProcessor<Allocations> GetProcessor(IServiceProvider services)
            => ActivatorUtilities.CreateInstance<Impl>(services);
    }

    public static implicit operator Allocations(ImmutableList<Allocation2> values)
        => new(values);
    
    private class Impl(IContext<CharityFractionSets> cCharityFractionSets) : EventProcessor<Allocations>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(cCharityFractionSets, previousContext, currentContext);
        }

        private sealed class Calc(IContext<CharityFractionSets> cCharityFractionSets, IContext previousContext, IContext currentContext)
            : BaseCalculation(previousContext, currentContext)
        {
            public ValueTask<CharityFractionSets> CurrentCharityFractionSets()
                => GetCurrent(cCharityFractionSets);

            protected override async ValueTask<Allocations> ConvExit(Allocations model, ConvExit e)
            {
                // Get the current charity fraction sets for this option
                FractionSet charityFractionSet = (await CurrentCharityFractionSets()).Shares[e.Option];
                
                // Create a new allocation record with the current timestamp, option ID, exit amount, and fraction sets
                var allocation = new Allocation2(
                    e.Timestamp,
                    e.Option,
                    e.Amount,
                    charityFractionSet
                );

                // Add the new allocation to the existing list of allocations
                return model.Values.Add(allocation);
            }
        }
    }
}
public record Allocation2(DateTimeOffset Timestamp, string OptionId, Real Amount, FractionSet Fractions);
