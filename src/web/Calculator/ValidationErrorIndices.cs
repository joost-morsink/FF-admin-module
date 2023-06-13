using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record ValidationErrorIndices(ImmutableList<int> Positions) : IModel<ValidationErrorIndices>
{
    public static ValidationErrorIndices Empty { get; } = new(ImmutableList<int>.Empty);
    public static IEventProcessor<ValidationErrorIndices> Processor { get; } = new Impl();
    public bool IsValid => Positions.IsEmpty;

    private class Impl : EventProcessor<ValidationErrorIndices>
    {
        public override ValidationErrorIndices Start { get; } = Empty;

        private ValidationErrorIndices Check(Func<bool> predicate, ValidationErrorIndices model, IContext context)
            => predicate()
                ? model
                : new(model.Positions.Add(context.GetContext<Index>().Value));

        protected override ValidationErrorIndices NewDonation(ValidationErrorIndices model, IHistoricContext context,
            NewDonation e)
            => Check(
                () => !context.Previous.IsDonationKnown(e.Donation) && context.Previous.IsOptionKnown(e.Option) &&
                      context.Previous.IsCharityKnown(e.Charity),
                model, context.Previous);

        protected override ValidationErrorIndices NewOption(ValidationErrorIndices model, IHistoricContext context, NewOption e)
            => Check(() => !context.Previous.IsOptionKnown(e.Code), model, context.Previous);

        protected override ValidationErrorIndices UpdateCharity(ValidationErrorIndices model, IHistoricContext context,
            UpdateCharity e)
            => Check(() => context.Previous.IsCharityKnown(e.Code), model, context.Previous);

        protected override ValidationErrorIndices UpdateFractions(ValidationErrorIndices model, IHistoricContext context,
            UpdateFractions e)
            => Check(() => context.Previous.IsOptionKnown(e.Code), model, context.Previous);

        protected override ValidationErrorIndices UpdateCharityForDonation(ValidationErrorIndices model,
            IHistoricContext context, UpdateCharityForDonation e)
            => Check(() => context.Previous.IsDonationKnown(e.Donation) && context.Previous.IsCharityKnown(e.Charity), model, context.Previous);

        protected override ValidationErrorIndices CharityPartition(ValidationErrorIndices model, IHistoricContext context,
            CharityPartition e)
            => Check(() => context.Previous.IsCharityKnown(e.Charity) &&
                           context.Previous.AreCharitiesKnown(e.Partitions.Select(p => p.Holder)), model, context.Previous);

        protected override ValidationErrorIndices CancelDonation(ValidationErrorIndices model, IHistoricContext context,
            CancelDonation e)
            => Check(() => context.Previous.IsDonationKnown(e.Donation), model, context.Previous);

        protected override ValidationErrorIndices ConvLiquidate(ValidationErrorIndices model, IHistoricContext context,
            ConvLiquidate e)
            => Check(() => context.Previous.IsOptionKnown(e.Option), model, context.Previous);

        protected override ValidationErrorIndices ConvExit(ValidationErrorIndices model, IHistoricContext context, ConvExit e)
            => Check(() => context.Previous.IsOptionKnown(e.Option), model, context.Previous);

        protected override ValidationErrorIndices ConvEnter(ValidationErrorIndices model, IHistoricContext context, ConvEnter e)
            => Check(() => context.Previous.IsOptionKnown(e.Option), model, context.Previous);

        protected override ValidationErrorIndices ConvInvest(ValidationErrorIndices model, IHistoricContext context,
            ConvInvest e)
            => Check(() => context.Previous.IsOptionKnown(e.Option), model, context.Previous);

        protected override ValidationErrorIndices ConvTransfer(ValidationErrorIndices model, IHistoricContext context,
            ConvTransfer e)
            => Check(() => context.Previous.IsCharityKnown(e.Charity), model, context.Previous);

        protected override ValidationErrorIndices IncreaseCash(ValidationErrorIndices model, IHistoricContext context,
            IncreaseCash e)
            => Check(() => context.Previous.IsOptionKnown(e.Option), model, context.Previous);
    }
}
