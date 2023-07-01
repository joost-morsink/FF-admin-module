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

        protected override ValidationErrors NewDonation(ValidationErrors model, IContext previousContext, IContext context,
            NewDonation e)
            => Check(
                () => !previousContext.IsDonationKnown(e.Donation) && previousContext.IsOptionKnown(e.Option) &&
                      previousContext.IsCharityKnown(e.Charity),
                "New donation must be to a known option and charity and must not be a duplicate",
                model, previousContext);

        protected override ValidationErrors NewOption(ValidationErrors model, IContext previousContext, IContext context, NewOption e)
            => Check(() => !previousContext.IsOptionKnown(e.Code),
                "New option must not be a duplicate", model, previousContext);

        protected override ValidationErrors UpdateCharity(ValidationErrors model, IContext previousContext, IContext context,
            UpdateCharity e)
            => Check(() => previousContext.IsCharityKnown(e.Code), 
                "Charity must be known to be updated", model, previousContext);

        protected override ValidationErrors UpdateFractions(ValidationErrors model, IContext previousContext, IContext context,
            UpdateFractions e)
            => Check(() => previousContext.IsOptionKnown(e.Code),
                "Option must be known to be updated", model, previousContext);

        protected override ValidationErrors UpdateCharityForDonation(ValidationErrors model,
            IContext previousContext, IContext context, UpdateCharityForDonation e)
            => Check(() => previousContext.IsDonationKnown(e.Donation) && previousContext.IsCharityKnown(e.Charity),
                "Donation and charity must be known to be updated", model, previousContext);

        protected override ValidationErrors CharityPartition(ValidationErrors model, IContext previousContext, IContext context,
            CharityPartition e)
            => Check(() => previousContext.IsCharityKnown(e.Charity) &&
                           previousContext.AreCharitiesKnown(e.Partitions.Select(p => p.Holder)), 
                "Charity and all holders must be known to perform partitioning", model, previousContext);

        protected override ValidationErrors CancelDonation(ValidationErrors model, IContext previousContext, IContext context,
            CancelDonation e)
            => Check(() => previousContext.IsDonationKnown(e.Donation),
                "Donation must be known to be cancelled", model, previousContext);

        protected override ValidationErrors ConvLiquidate(ValidationErrors model, IContext previousContext, IContext context,
            ConvLiquidate e)
            => Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to execute liquidate", model, previousContext);
                
        protected override ValidationErrors ConvExit(ValidationErrors model, IContext previousContext, IContext context, ConvExit e)
            => Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to execute exit", model, previousContext);

        protected override ValidationErrors ConvEnter(ValidationErrors model, IContext previousContext, IContext context, ConvEnter e)
            => Check(() => previousContext.IsOptionKnown(e.Option), 
                "Option must be known to execute enter", model, previousContext);

        protected override ValidationErrors ConvInvest(ValidationErrors model, IContext previousContext, IContext context,
            ConvInvest e)
            => Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to execute invest", model, previousContext);

        protected override ValidationErrors ConvTransfer(ValidationErrors model, IContext previousContext, IContext context,
            ConvTransfer e)
            => Check(() => previousContext.IsCharityKnown(e.Charity), 
                "Charity must be known to transfer money", model, previousContext);

        protected override ValidationErrors IncreaseCash(ValidationErrors model, IContext previousContext, IContext context,
            IncreaseCash e)
            => Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to increase cash", model, previousContext);
    }
}
public record ValidationError(int Position, string Message);
