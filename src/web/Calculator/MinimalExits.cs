using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record MinimalExits(ImmutableDictionary<string, Real> Exits) : IModel<MinimalExits>
{
    public static implicit operator MinimalExits(ImmutableDictionary<string, Real> exits)
        => new(exits);
    public static MinimalExits Empty { get; } = new(ImmutableDictionary<string, decimal>.Empty);

    public static IEventProcessor<MinimalExits> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    private class Impl(IContext<Options> cOptions, IContext<IdealOptionValuations> cIdealOptionValuations) : EventProcessor<MinimalExits>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptions, cIdealOptionValuations);
        }

        private sealed class Calc(
            IContext previousContext,
            IContext currentContext,
            IContext<Options> cOptions,
            IContext<IdealOptionValuations> cIdealOptionValuations) : BaseCalculation(previousContext, currentContext)
        {
            public Options CurrentOptions => GetCurrent(cOptions);
            public IdealOptionValuations CurrentIdealOptionValuations => GetCurrent(cIdealOptionValuations);

            protected override MinimalExits ConvLiquidate(MinimalExits model, ConvLiquidate e) 
                => CalculateNewMinimalExits(model, e.Option, e.Timestamp);

            protected override MinimalExits PriceInfo(MinimalExits model, PriceInfo e)
                => CalculateNewMinimalExits(model, e.Option, e.Timestamp);
            
            private MinimalExits CalculateNewMinimalExits(MinimalExits model, string optionId, DateTimeOffset timestamp)
            {
                var option = CurrentOptions.Values[optionId];
                var valuations = CurrentIdealOptionValuations.Valuations[optionId];
                var yearsSinceLastExit = (timestamp - valuations.Timestamp).TotalDays / 365.25;
                var percentage = Math.Pow(1 + (double)option.BadYearFraction, yearsSinceLastExit) - 1;
                var minExit = valuations.RealValue * (Real)percentage;
                return new(model.Exits.SetItem(optionId, minExit));
            }
        }
    }
}

