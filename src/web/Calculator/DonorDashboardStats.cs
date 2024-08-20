using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record DonorDashboardStats(ImmutableDictionary<string, DonorDashboardStat> Donors) : IModel<DonorDashboardStats>
{
    public static implicit operator DonorDashboardStats(ImmutableDictionary<string,DonorDashboardStat> dict)
        => new(dict);
    public static DonorDashboardStats Empty { get; } = new(ImmutableDictionary<string, DonorDashboardStat>.Empty);

    public static IEventProcessor<DonorDashboardStats> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);
    private class Impl(IContext<Donors> cDonors, IContext<DonationRecords> cDonationRecords) : EventProcessor<DonorDashboardStats>
    {
        public override DonorDashboardStats Process(DonorDashboardStats model, IContext previousContext, IContext context,
            Event e)
        {
            if (ShouldCalculate(e))
                return base.Process(model, previousContext, context, e);
            return model;
        }
        public bool ShouldCalculate(Event e)
        {
            return e.Type is EventType.CONV_EXIT or EventType.CONV_ENTER or EventType.PRICE_INFO
                or EventType.CONV_INVEST
                or EventType.CONV_LIQUIDATE or EventType.CONV_INCREASE_CASH;
        }

        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cDonors, cDonationRecords);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Donors> cDonors, IContext<DonationRecords> cDonationRecords)
            : BaseCalculation(previousContext, currentContext)
        {
            public Donors CurrentDonors => GetCurrent(cDonors);
            public DonationRecords CurrentDonationRecords => GetCurrent(cDonationRecords);
            
            protected override DonorDashboardStats Default(DonorDashboardStats model, Event e)
            {
                return Calculate();
            }
            private DonorDashboardStats Calculate()
            {

                var donors = CurrentDonors;
                var donationRecords = CurrentDonationRecords;
                var result = donors.Values.ToImmutableDictionary(d => d.Key,
                    d => new DonorDashboardStat(d.Value
                        .Where(r => donationRecords.Values.ContainsKey(r))
                        .ToImmutableDictionary(r => r, r => donationRecords.Values[r])));
                return result;
            }
        }
    }
}

public record DonorDashboardStat(ImmutableDictionary<string, ImmutableList<DonationRecord>> Donations);
