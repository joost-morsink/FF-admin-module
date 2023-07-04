namespace FfAdmin.Calculator.Core;

public abstract class ContextualCalculator<T> : IContextualCalculator<T>
    where T : class
{
    object IEventProcessor.Start => Start;

    object IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => Process(previousContext, context, e);

    public abstract T Start { get; }
    public Type ModelType => typeof(T);
    public virtual IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();

    public virtual T Process(IContext previousContext, IContext context, Event e)
        => e switch
        {
            NewOption no => NewOption(previousContext, context, no),
            UpdateFractions uf => UpdateFractions(previousContext, context, uf),
            NewCharity nc => NewCharity(previousContext, context, nc),
            UpdateCharity uc => UpdateCharity(previousContext, context, uc),
            CharityPartition cp => CharityPartition(previousContext, context, cp),
            NewDonation nd => NewDonation(previousContext, context, nd),
            UpdateCharityForDonation ucd => UpdateCharityForDonation(previousContext, context, ucd),
            CancelDonation cd => CancelDonation(previousContext, context, cd),
            ConvLiquidate cl => ConvLiquidate(previousContext, context, cl),
            ConvExit ce => ConvExit(previousContext, context, ce),
            ConvTransfer ct => ConvTransfer(previousContext, context, ct),
            ConvEnter ce => ConvEnter(previousContext, context, ce),
            ConvInvest ci => ConvInvest(previousContext, context, ci),
            IncreaseCash ic => IncreaseCash(previousContext, context, ic),
            Audit a => Audit(previousContext, context, a),
            _ => Default(context, e)
        };

    protected virtual T NewOption(IContext context, NewOption e)
        => Default(context, e);

    protected virtual T NewOption(IContext previousContext, IContext context, NewOption e)
        => NewOption(context, e);

    protected virtual T UpdateFractions(IContext context, UpdateFractions e)
        => Default(context, e);

    protected virtual T UpdateFractions(IContext previousContext, IContext context, UpdateFractions e)
        => UpdateFractions( context, e);

    protected virtual T NewCharity(IContext context, NewCharity e)
        => Default(context, e);

    protected virtual T NewCharity(IContext previousContext, IContext context, NewCharity e)
        => NewCharity(context, e);

    protected virtual T UpdateCharity(IContext context, UpdateCharity e)
        => Default(context, e);

    protected virtual T UpdateCharity( IContext previousContext, IContext context, UpdateCharity e)
        => UpdateCharity(context, e);

    protected virtual T CharityPartition(IContext context, CharityPartition e)
        => Default(context, e);

    protected virtual T CharityPartition(IContext previousContext, IContext context, CharityPartition e)
        => CharityPartition(context, e);

    protected virtual T NewDonation(IContext context, NewDonation e)
        => Default( context, e);

    protected virtual T NewDonation(IContext previousContext, IContext context, NewDonation e)
        => NewDonation(context, e);

    protected virtual T UpdateCharityForDonation( IContext context, UpdateCharityForDonation e)
        => Default( context, e);

    protected virtual T UpdateCharityForDonation(IContext previousContext, IContext context, UpdateCharityForDonation e)
        => UpdateCharityForDonation( context, e);

    protected virtual T CancelDonation(IContext context, CancelDonation e)
        => Default(context, e);

    protected virtual T CancelDonation( IContext previousContext, IContext context, CancelDonation e)
        => CancelDonation( context, e);

    protected virtual T ConvLiquidate(IContext context, ConvLiquidate e)
        => Default(context, e);

    protected virtual T ConvLiquidate( IContext previousContext, IContext context, ConvLiquidate e)
        => ConvLiquidate( context, e);

    protected virtual T ConvExit( IContext context, ConvExit e)
        => Default(context, e);

    protected virtual T ConvExit( IContext previousContext, IContext context, ConvExit e)
        => ConvExit(context, e);

    protected virtual T ConvTransfer( IContext context, ConvTransfer e)
        => Default( context, e);

    protected virtual T ConvTransfer( IContext previousContext, IContext context, ConvTransfer e)
        => ConvTransfer(context, e);

    protected virtual T ConvEnter(IContext context, ConvEnter e)
        => Default( context, e);

    protected virtual T ConvEnter(IContext previousContext, IContext context, ConvEnter e)
        => ConvEnter(context, e);

    protected virtual T ConvInvest(IContext context, ConvInvest e)
        => Default( context, e);

    protected virtual T ConvInvest(IContext previousContext, IContext context, ConvInvest e)
        => ConvInvest( context, e);

    protected virtual T IncreaseCash(IContext context, IncreaseCash e)
        => Default(context, e);

    protected virtual T IncreaseCash(IContext previousContext, IContext context, IncreaseCash e)
        => IncreaseCash( context, e);

    protected virtual T Audit( IContext context, Audit e)
        => Default(context, e);

    protected virtual T Audit(IContext previousContext, IContext context, Audit e)
        => Audit(context, e);

    protected virtual T Default(IContext context, Event e)
        => Start;
}
