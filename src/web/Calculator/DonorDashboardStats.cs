namespace FfAdmin.Calculator;

public record DonorDashboardStats(ImmutableDictionary<string, DonorDashboardStat> Donors) : IModel<DonorDashboardStats>
{
    public static implicit operator DonorDashboardStats(ImmutableDictionary<string,DonorDashboardStat> dict)
        => new(dict);
    public static DonorDashboardStats Empty { get; } = new(ImmutableDictionary<string, DonorDashboardStat>.Empty);
    public static IEventProcessor<DonorDashboardStats> Processor { get; } = new Impl();

    private class Impl : IEventProcessor<DonorDashboardStats>
    {
        public DonorDashboardStats Start => Empty;

        public IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();

        public DonorDashboardStats Process(DonorDashboardStats model, IContext previousContext, IContext context,
            Event e)
        {
            if (ShouldCalculate(e))
                return Calculate(context);
            return model;
        }

        public bool ShouldCalculate(Event e)
        {
            return e.Type is EventType.CONV_EXIT or EventType.CONV_ENTER or EventType.PRICE_INFO
                or EventType.CONV_INVEST
                or EventType.CONV_LIQUIDATE or EventType.CONV_INCREASE_CASH;
        }

        public DonorDashboardStats Calculate(IContext context)
        {
            var donors = context.GetContext<Donors>();
            var donationRecords = context.GetContext<DonationRecords>();
            var result = donors.Values.ToImmutableDictionary(d => d.Key,
                d => new DonorDashboardStat(d.Value
                    .Where(r => donationRecords.Values.ContainsKey(r))
                    .ToImmutableDictionary(r => r, r => donationRecords.Values[r])));
            return result;
        }
    }
}

public record DonorDashboardStat(ImmutableDictionary<string, ImmutableList<DonationRecord>> Donations);
