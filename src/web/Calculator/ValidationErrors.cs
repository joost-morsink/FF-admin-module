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
            => model.Check(
                () => !previousContext.IsDonationKnown(e.Donation) && previousContext.IsOptionKnown(e.Option) &&
                      previousContext.IsCharityKnown(e.Charity),
                "New donation must be to a known option and charity and must not be a duplicate",
                      previousContext);

        protected override ValidationErrors NewOption(ValidationErrors model, IContext previousContext, IContext context, NewOption e)
            => model.Check(() => !previousContext.IsOptionKnown(e.Code),
                "New option must not be a duplicate", previousContext);

        protected override ValidationErrors UpdateCharity(ValidationErrors model, IContext previousContext, IContext context,
            UpdateCharity e)
            => model.Check(() => previousContext.IsCharityKnown(e.Code), 
                "Charity must be known to be updated", previousContext);

        protected override ValidationErrors UpdateFractions(ValidationErrors model, IContext previousContext, IContext context,
            UpdateFractions e)
            => model.Check(() => previousContext.IsOptionKnown(e.Code),
                "Option must be known to be updated", previousContext);

        protected override ValidationErrors UpdateCharityForDonation(ValidationErrors model,
            IContext previousContext, IContext context, UpdateCharityForDonation e)
            => model.Check(() => previousContext.IsDonationKnown(e.Donation) && previousContext.IsCharityKnown(e.Charity),
                "Donation and charity must be known to be updated", previousContext);

        protected override ValidationErrors CharityPartition(ValidationErrors model, IContext previousContext, IContext context,
            CharityPartition e)
            => model.Check(() => previousContext.IsCharityKnown(e.Charity) &&
                           previousContext.AreCharitiesKnown(e.Partitions.Select(p => p.Holder)), 
                "Charity and all holders must be known to perform partitioning", previousContext);

        protected override ValidationErrors CancelDonation(ValidationErrors model, IContext previousContext, IContext context,
            CancelDonation e)
            => model.Check(() => previousContext.IsDonationKnown(e.Donation),
                "Donation must be known to be cancelled", previousContext);

        protected override ValidationErrors ConvLiquidate(ValidationErrors model, IContext previousContext, IContext context,
            ConvLiquidate e)
            => model.Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to execute liquidate", previousContext);
                
        protected override ValidationErrors ConvExit(ValidationErrors model, IContext previousContext, IContext context, ConvExit e)
            => model.Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to execute exit", previousContext)
                .CheckCharityBalance(context);

        protected override ValidationErrors ConvEnter(ValidationErrors model, IContext previousContext, IContext context, ConvEnter e)
            => model.Check(() => previousContext.IsOptionKnown(e.Option), 
                "Option must be known to execute enter", previousContext);

        protected override ValidationErrors ConvInvest(ValidationErrors model, IContext previousContext, IContext context,
            ConvInvest e)
            => model.Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to execute invest", previousContext);

        protected override ValidationErrors ConvTransfer(ValidationErrors model, IContext previousContext, IContext context,
            ConvTransfer e)
            => model.Check(() => previousContext.IsCharityKnown(e.Charity), 
                "Charity must be known to transfer money", previousContext)
                .CheckCharityBalance(context);

        protected override ValidationErrors IncreaseCash(ValidationErrors model, IContext previousContext, IContext context,
            IncreaseCash e)
            => model.Check(() => previousContext.IsOptionKnown(e.Option),
                "Option must be known to increase cash", previousContext);
        
    }
}

internal static class ValidationErrorsExt
{
    public static ValidationErrors Check(this ValidationErrors model, Func<bool> predicate, string message, IContext context)
        => predicate()
            ? model
            : new(model.Errors.Add(new(context.GetContext<Index>().Value, message)));
    public static ValidationErrors CheckCharityBalance(this ValidationErrors model, IContext context) 
        => model.Check(() => Math.Abs(context.GetContext<CharityBalance>().Amount - context.GetContext<AmountsToTransfer>().Values.Values.Sum()) < (Real)0.0001, 
            "Charity balance must be valid", context);
}
public record ValidationError(int Position, string Message);
