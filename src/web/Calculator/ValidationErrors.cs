using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record ValidationErrors(ImmutableList<ValidationError> Errors) : IModel<ValidationErrors>
{
    public static ValidationErrors Empty { get; } = new(ImmutableList<ValidationError>.Empty);

    public static IEventProcessor<ValidationErrors> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    public bool IsValid => Errors.IsEmpty;

    private class Impl(IContext<Index> cIndex, IContext<Donations> cDonations, IContext<Options> cOptions, IContext<Charities> cCharities, IContext<CharityBalance> cCharityBalance, IContext<AmountsToTransfer> cAmountsToTransfer) : EventProcessor<ValidationErrors>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cIndex, cDonations, cOptions, cCharities, cCharityBalance, cAmountsToTransfer);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Index> cIndex, IContext<Donations> cDonations, IContext<Options> cOptions, IContext<Charities> cCharities, IContext<CharityBalance> cCharityBalance, IContext<AmountsToTransfer> cAmountsToTransfer)
            : BaseCalculation(previousContext, currentContext)
        {
            public Index CurrentIndex => GetCurrent(cIndex);
            public Index PreviousIndex => GetPrevious(cIndex);
            public Donations PreviousDonations => GetPrevious(cDonations);
            public Options PreviousOptions => GetPrevious(cOptions);
            public Charities PreviousCharities => GetPrevious(cCharities);
            public CharityBalance CurrentCharityBalance => GetCurrent(cCharityBalance);
            public AmountsToTransfer CurrentAmountsToTransfer => GetCurrent(cAmountsToTransfer);
            private ValidationErrors Check(Func<bool> predicate, string message, ValidationErrors model)
                => predicate()
                    ? model
                    : new(model.Errors.Add(new(PreviousIndex.Value, message)));

            protected override ValidationErrors NewDonation(ValidationErrors model, NewDonation e)
                => model.Check(
                    () => !PreviousDonations.Contains(e.Donation) && PreviousOptions.Contains(e.Option) &&
                          PreviousCharities.Contains(e.Charity),
                    "New donation must be to a known option and charity and must not be a duplicate",
                    PreviousIndex);

            protected override ValidationErrors NewOption(ValidationErrors model, NewOption e)
                => model.Check(() => !PreviousOptions.Contains(e.Code),
                    "New option must not be a duplicate", PreviousIndex);

            protected override ValidationErrors UpdateCharity(ValidationErrors model, UpdateCharity e)
                => model.Check(() => PreviousCharities.Contains(e.Code),
                    "Charity must be known to be updated", PreviousIndex);

            protected override ValidationErrors UpdateFractions(ValidationErrors model, UpdateFractions e)
                => model.Check(() => PreviousOptions.Contains(e.Code),
                    "Option must be known to be updated", PreviousIndex);

            protected override ValidationErrors UpdateCharityForDonation(ValidationErrors model, UpdateCharityForDonation e)
                => model.Check(() => PreviousDonations.Contains(e.Donation) && PreviousCharities.Contains(e.Charity),
                    "Donation and charity must be known to be updated", PreviousIndex);

            protected override ValidationErrors CharityPartition(ValidationErrors model, CharityPartition e)
                => model.Check(() =>  PreviousCharities.Contains(e.Charity) &&
                                      e.Partitions.Select(p => p.Holder).All(PreviousCharities.Contains),
                    "Charity and all holders must be known to perform partitioning", PreviousIndex);

            protected override ValidationErrors CancelDonation(ValidationErrors model, CancelDonation e)
                => model.Check(() => PreviousDonations.Contains(e.Donation),
                    "Donation must be known to be cancelled", PreviousIndex);

            protected override ValidationErrors ConvLiquidate(ValidationErrors model, ConvLiquidate e)
                => model.Check(() => PreviousOptions.Contains(e.Option),
                    "Option must be known to execute liquidate", PreviousIndex);

            protected override ValidationErrors ConvExit(ValidationErrors model, ConvExit e)
                => model.Check(() => PreviousOptions.Contains(e.Option),
                        "Option must be known to execute exit", PreviousIndex)
                    .CheckCharityBalance(CurrentCharityBalance, CurrentAmountsToTransfer, PreviousIndex);

            protected override ValidationErrors ConvEnter(ValidationErrors model, ConvEnter e)
                => model.Check(() => PreviousOptions.Contains(e.Option),
                    "Option must be known to execute enter", PreviousIndex);

            protected override ValidationErrors ConvInvest(ValidationErrors model, ConvInvest e)
                => model.Check(() => PreviousOptions.Contains(e.Option),
                    "Option must be known to execute invest", PreviousIndex);

            protected override ValidationErrors ConvTransfer(ValidationErrors model, ConvTransfer e)
                => model.Check(() => PreviousCharities.Contains(e.Charity),
                        "Charity must be known to transfer money", PreviousIndex)
                    .CheckCharityBalance(CurrentCharityBalance, CurrentAmountsToTransfer, PreviousIndex);

            protected override ValidationErrors IncreaseCash(ValidationErrors model, IncreaseCash e)
                => model.Check(() => PreviousOptions.Contains(e.Option),
                    "Option must be known to increase cash", PreviousIndex);
        }
    }
}

internal static class ValidationErrorsExt
{
    public static ValidationErrors Check(this ValidationErrors model, Func<bool> predicate, string message, Index index)
        => predicate()
            ? model
            : new(model.Errors.Add(new(index.Value, message)));
    public static ValidationErrors CheckCharityBalance(this ValidationErrors model, CharityBalance charityBalance, AmountsToTransfer amountsToTransfer, Index index) 
        => model.Check(() => Math.Abs(charityBalance.Amount - amountsToTransfer.Values.Values.SelectMany(mb => mb.Amounts.Values).Sum()) < (Real)0.0001, 
            "Charity balance must be valid", index);
}
public record ValidationError(int Position, string Message);
