using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record ValidationErrors(ImmutableList<ValidationError> Errors) : IModel<ValidationErrors>
{
    public static ValidationErrors Empty { get; } = new(ImmutableList<ValidationError>.Empty);
    public static IEventProcessor<ValidationErrors> Processor { get; } = new Impl();
    public bool IsValid => Errors.IsEmpty;

    private class Impl : EventProcessor<ValidationErrors>
    {
        public override ValidationErrors Start => Empty;

        private ValidationErrors Check(Func<bool> predicate, string message, ValidationErrors model, IContext context)
            => predicate()
                ? model
                : new(model.Errors.Add(new(context.GetContext<Index>().Value, message)));

        protected override ValidationErrors NewDonation(ValidationErrors model, IHistoricContext context,
            NewDonation e)
            => Check(
                () => !context.Previous.IsDonationKnown(e.Donation) && context.Previous.IsOptionKnown(e.Option) &&
                      context.Previous.IsCharityKnown(e.Charity),
                "New donation must be to a known option and charity and must not be a duplicate",
                model, context.Previous);

        protected override ValidationErrors NewOption(ValidationErrors model, IHistoricContext context, NewOption e)
            => Check(() => !context.Previous.IsOptionKnown(e.Code),
                "New option must not be a duplicate", model, context.Previous);

        protected override ValidationErrors UpdateCharity(ValidationErrors model, IHistoricContext context,
            UpdateCharity e)
            => Check(() => context.Previous.IsCharityKnown(e.Code), 
                "Charity must be known to be updated", model, context.Previous);

        protected override ValidationErrors UpdateFractions(ValidationErrors model, IHistoricContext context,
            UpdateFractions e)
            => Check(() => context.Previous.IsOptionKnown(e.Code),
                "Option must be known to be updated", model, context.Previous);

        protected override ValidationErrors UpdateCharityForDonation(ValidationErrors model,
            IHistoricContext context, UpdateCharityForDonation e)
            => Check(() => context.Previous.IsDonationKnown(e.Donation) && context.Previous.IsCharityKnown(e.Charity),
                "Donation and charity must be known to be updated", model, context.Previous);

        protected override ValidationErrors CharityPartition(ValidationErrors model, IHistoricContext context,
            CharityPartition e)
            => Check(() => context.Previous.IsCharityKnown(e.Charity) &&
                           context.Previous.AreCharitiesKnown(e.Partitions.Select(p => p.Holder)), 
                "Charity and all holders must be known to perform partitioning", model, context.Previous);

        protected override ValidationErrors CancelDonation(ValidationErrors model, IHistoricContext context,
            CancelDonation e)
            => Check(() => context.Previous.IsDonationKnown(e.Donation),
                "Donation must be known to be cancelled", model, context.Previous);

        protected override ValidationErrors ConvLiquidate(ValidationErrors model, IHistoricContext context,
            ConvLiquidate e)
            => Check(() => context.Previous.IsOptionKnown(e.Option),
                "Option must be known to execute liquidate", model, context.Previous);
                
        protected override ValidationErrors ConvExit(ValidationErrors model, IHistoricContext context, ConvExit e)
            => Check(() => context.Previous.IsOptionKnown(e.Option),
                "Option must be known to execute exit", model, context.Previous);

        protected override ValidationErrors ConvEnter(ValidationErrors model, IHistoricContext context, ConvEnter e)
            => Check(() => context.Previous.IsOptionKnown(e.Option), 
                "Option must be known to execute enter", model, context.Previous);

        protected override ValidationErrors ConvInvest(ValidationErrors model, IHistoricContext context,
            ConvInvest e)
            => Check(() => context.Previous.IsOptionKnown(e.Option),
                "Option must be known to execute invest", model, context.Previous);

        protected override ValidationErrors ConvTransfer(ValidationErrors model, IHistoricContext context,
            ConvTransfer e)
            => Check(() => context.Previous.IsCharityKnown(e.Charity), 
                "Charity must be known to transfer money", model, context.Previous);

        protected override ValidationErrors IncreaseCash(ValidationErrors model, IHistoricContext context,
            IncreaseCash e)
            => Check(() => context.Previous.IsOptionKnown(e.Option),
                "Option must be known to increase cash", model, context.Previous);
    }
}
public record ValidationError(int Position, string Message);
