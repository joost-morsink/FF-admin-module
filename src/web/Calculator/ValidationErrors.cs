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
            public ValueTask<Index> CurrentIndex => GetCurrent(cIndex);
            public ValueTask<Index> PreviousIndex => GetPrevious(cIndex);
            public ValueTask<Donations> PreviousDonations => GetPrevious(cDonations);
            public ValueTask<Options> PreviousOptions => GetPrevious(cOptions);
            public ValueTask<Charities> PreviousCharities => GetPrevious(cCharities);
            public ValueTask<CharityBalance> CurrentCharityBalance => GetCurrent(cCharityBalance);
            public ValueTask<AmountsToTransfer> CurrentAmountsToTransfer => GetCurrent(cAmountsToTransfer);
            private async ValueTask<ValidationErrors> Check(Func<bool> predicate, string message, ValidationErrors model)
                => predicate()
                    ? model
                    : new(model.Errors.Add(new((await PreviousIndex).Value, message)));

            protected override async ValueTask<ValidationErrors> NewDonation(ValidationErrors model, NewDonation e)
                => await model.Check(
                    async () => !(await PreviousDonations).Contains(e.Donation) && (await PreviousOptions).Contains(e.Option) &&
                          (await PreviousCharities).Contains(e.Charity),
                    "New donation must be to a known option and charity and must not be a duplicate",
                    await PreviousIndex);

            protected override async ValueTask<ValidationErrors> NewOption(ValidationErrors model, NewOption e)
                => await model.Check(async () => !(await PreviousOptions).Contains(e.Code),
                    "New option must not be a duplicate", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> UpdateCharity(ValidationErrors model, UpdateCharity e)
                => await model.Check(async () => (await PreviousCharities).Contains(e.Code),
                    "Charity must be known to be updated", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> UpdateFractions(ValidationErrors model, UpdateFractions e)
                => await model.Check(async () => (await PreviousOptions).Contains(e.Code),
                    "Option must be known to be updated", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> UpdateCharityForDonation(ValidationErrors model, UpdateCharityForDonation e)
                => await model.Check(async () => (await PreviousDonations).Contains(e.Donation) && (await PreviousCharities).Contains(e.Charity),
                    "Donation and charity must be known to be updated", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> CharityPartition(ValidationErrors model, CharityPartition e)
                => await model.Check(async () => (await PreviousCharities).Contains(e.Charity) &&
                                     e.Partitions.Select(p => p.Holder).All((await PreviousCharities).Contains),
                    "Charity and all holders must be known to perform partitioning", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> CancelDonation(ValidationErrors model, CancelDonation e)
                => await model.Check(async () => (await PreviousDonations).Contains(e.Donation),
                    "Donation must be known to be cancelled", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> ConvLiquidate(ValidationErrors model, ConvLiquidate e)
                => await model.Check(async () => (await PreviousOptions).Contains(e.Option),
                    "Option must be known to execute liquidate", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> ConvExit(ValidationErrors model, ConvExit e)
                => await (await model.Check(async () => (await PreviousOptions).Contains(e.Option),
                        "Option must be known to execute exit", await PreviousIndex))
                    .CheckCharityBalance(await CurrentCharityBalance, await CurrentAmountsToTransfer, await PreviousIndex);

            protected override async ValueTask<ValidationErrors> ConvEnter(ValidationErrors model, ConvEnter e)
                => await model.Check(async () => (await PreviousOptions).Contains(e.Option),
                    "Option must be known to execute enter", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> ConvInvest(ValidationErrors model, ConvInvest e)
                => await model.Check(async () => (await PreviousOptions).Contains(e.Option),
                    "Option must be known to execute invest", await PreviousIndex);

            protected override async ValueTask<ValidationErrors> ConvTransfer(ValidationErrors model, ConvTransfer e)
                => await (await model.Check(async () => (await PreviousCharities).Contains(e.Charity),
                        "Charity must be known to transfer money", await PreviousIndex))
                    .CheckCharityBalance(await CurrentCharityBalance, await CurrentAmountsToTransfer, await PreviousIndex);

            protected override async ValueTask<ValidationErrors> IncreaseCash(ValidationErrors model, IncreaseCash e)
                => await model.Check(async () => (await PreviousOptions).Contains(e.Option),
                    "Option must be known to increase cash", await PreviousIndex);
        }
    }
}

internal static class ValidationErrorsExt
{
    public static async ValueTask<ValidationErrors> Check(this ValidationErrors model, Func<ValueTask<bool>> predicate, string message, Index index)
        => await predicate()
            ? model
            : new(model.Errors.Add(new(index.Value, message)));
    public static ValueTask<ValidationErrors> CheckCharityBalance(this ValidationErrors model, CharityBalance charityBalance, AmountsToTransfer amountsToTransfer, Index index) 
        => model.Check(async () => Math.Abs(charityBalance.Amount - amountsToTransfer.Values.Values.SelectMany(mb => mb.Amounts.Values).Sum()) < (Real)0.0001, 
            "Charity balance must be valid", index);
}
public record ValidationError(int Position, string Message);
