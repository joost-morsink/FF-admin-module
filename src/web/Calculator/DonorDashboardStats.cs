using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record DonorDashboardStats(ImmutableDictionary<string, DonorDashboardStat> Donors) : IModel<DonorDashboardStats>
{
    public static IMetaModel<DonorDashboardStats> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<DonorDashboardStats>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override DonorDashboardStats Empty { get; } = new(ImmutableDictionary<string, DonorDashboardStat>.Empty);
        public override IEventProcessor<DonorDashboardStats> GetProcessor(IServiceProvider services)
            => ActivatorUtilities.CreateInstance<Impl>(services);
    }
    public static implicit operator DonorDashboardStats(ImmutableDictionary<string,DonorDashboardStat> dict)
        => new(dict);
    private class Impl(IContext<Donors> cDonors, IContext<DonationRecords> cDonationRecords) : EventProcessor<DonorDashboardStats>
    {
        public override async ValueTask<DonorDashboardStats> Process(DonorDashboardStats model, IContext previousContext, IContext context,
            Event e)
        {
            if (ShouldCalculate(e))
                return await base.Process(model, previousContext, context, e);
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
            public ValueTask<Donors> CurrentDonors => GetCurrent(cDonors);
            public ValueTask<DonationRecords> CurrentDonationRecords => GetCurrent(cDonationRecords);
            
            protected override ValueTask<DonorDashboardStats> Default(DonorDashboardStats model, Event e)
            {
                return Calculate();
            }
            private async ValueTask<DonorDashboardStats> Calculate()
            {

                var donors = await CurrentDonors;
                var donationRecords = await CurrentDonationRecords;
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

public class DonorDashboardStats2(IContext<Donors2,string, Donors2.Details> cDonors,
    IContext<Donations2, string, Donations2.Details> cDonations,
    IModelCalculator<DonationRecords2.Value, string> cDonationRecords) : IModelCalculator<DonorDashboardStats2.Stat, string>
{
    public record Stat(ImmutableDictionary<string, StatDetail> Donations);

    public record StatDetail(Donation Donation, ImmutableList<DonationRecord2> Records);
    public async ValueTask<Stat> Calculate(IContext context, string parameter)
    {
        var donations = (await cDonors.GetValue(context, parameter)).Values[parameter];
        var donationRecords = await Task.WhenAll(
            donations.Select(async d => ((await cDonations.GetValue(context,d)).Values[d], await cDonationRecords.Calculate(context, d))));
        var result = donationRecords.ToImmutableDictionary(t => t.Item1.Id, t => new StatDetail(t.Item1, t.Item2.Records));
        return new(result);
    }
}
