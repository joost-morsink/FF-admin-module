using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record AuditHistory(ImmutableList<AuditMoment> Moments) : IModel<AuditHistory>
{
    public static implicit operator AuditHistory(ImmutableList<AuditMoment> moments)
        => new(moments);
    public static AuditHistory Empty { get; } = new(ImmutableList<AuditMoment>.Empty);

    public static IEventProcessor<AuditHistory> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    public AuditHistory Add(AuditMoment moment)
        => Moments.Add(moment);
    private class Impl(IContext<Index> cIndex, IContext<HistoryHash> cHistoryHash) : EventProcessor<AuditHistory>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cIndex, cHistoryHash);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Index> cIndex, IContext<HistoryHash> cHistoryHash)
            : BaseCalculation(previousContext, currentContext)
        {
            public Index PreviousIndex => GetPrevious(cIndex);
            public HistoryHash PreviousHash => GetPrevious(cHistoryHash);
            protected override AuditHistory Audit(AuditHistory model, Audit e)
            {
                var index = PreviousIndex.Value;
                var hash = Convert.ToBase64String(PreviousHash.Hash);
                var prev = model.Moments.LastOrDefault();

                var valid = e.EventCount == index && e.Hashcode == hash &&
                            (prev is null
                                ? !e.PreviousCount.HasValue && string.IsNullOrWhiteSpace(e.PreviousHashCode)
                                : e.PreviousCount.HasValue && e.PreviousCount == prev.EventCount &&
                                  e.PreviousHashCode is not null && e.PreviousHashCode == prev.HashCode);

                return model.Add(new(e.Timestamp, valid, e.Hashcode, e.EventCount, e.PreviousHashCode, e.PreviousCount));
            }
        }
    }
}

public record AuditMoment(DateTimeOffset Timestamp, bool Valid, string HashCode, int EventCount, string? PreviousHashCode, int? PreviousCount);

