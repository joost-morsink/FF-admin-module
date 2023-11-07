namespace FfAdmin.Calculator;

public record AuditHistory(ImmutableList<AuditMoment> Moments) : IModel<AuditHistory>
{
    public static implicit operator AuditHistory(ImmutableList<AuditMoment> moments)
        => new(moments);
    public static AuditHistory Empty { get; } = new(ImmutableList<AuditMoment>.Empty);
    public static IEventProcessor<AuditHistory> Processor { get; } = new Impl();

    public AuditHistory Add(AuditMoment moment)
        => Moments.Add(moment);
    private class Impl : EventProcessor<AuditHistory>
    {
        public override AuditHistory Start => Empty;
        protected override AuditHistory Audit(AuditHistory model, IContext previousContext, IContext context, Audit e)
        {
            var index = previousContext.GetContext<Index>().Value;
            var hash = Convert.ToBase64String(previousContext.GetContext<HistoryHash>().Hash);
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

public record AuditMoment(DateTimeOffset Timestamp, bool Valid, string HashCode, int EventCount, string? PreviousHashCode, int? PreviousCount);

