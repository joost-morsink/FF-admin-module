namespace FfAdmin.Calculator.Core;

public abstract class ContextualCalculator<T> : IContextualCalculator<T>
    where T : class
{
    object IEventProcessor.Start => Start;

    object IEventProcessor.Process(object model, IHistoricContext historicContext, Event e)
        => Process(historicContext, e);

    public abstract T Start { get; }
    public Type ModelType => typeof(T);

    public virtual T Process(IHistoricContext historicContext, Event e)
        => e switch
        {
            NewOption no => NewOption(historicContext, no),
            UpdateFractions uf => UpdateFractions(historicContext, uf),
            NewCharity nc => NewCharity(historicContext, nc),
            UpdateCharity uc => UpdateCharity(historicContext, uc),
            CharityPartition cp => CharityPartition(historicContext, cp),
            NewDonation nd => NewDonation(historicContext, nd),
            UpdateCharityForDonation ucd => UpdateCharityForDonation(historicContext, ucd),
            CancelDonation cd => CancelDonation(historicContext, cd),
            ConvLiquidate cl => ConvLiquidate(historicContext, cl),
            ConvExit ce => ConvExit(historicContext, ce),
            ConvTransfer ct => ConvTransfer(historicContext, ct),
            ConvEnter ce => ConvEnter(historicContext, ce),
            ConvInvest ci => ConvInvest(historicContext, ci),
            IncreaseCash ic => IncreaseCash(historicContext, ic),
            Audit a => Audit(historicContext, a),
            _ => Default(historicContext.Current, e)
        };

    public virtual Func<int, T> PositionalModelCreator(EventStream stream)
        => position =>
        {
            if (position == 0)
                return Start;
            var historicContext = stream.GetHistoricContext(position);
            return Process(historicContext, stream.Events[position - 1]);
        };

    Delegate IEventProcessor.PositionalModelCreator(EventStream stream)
        => PositionalModelCreator(stream);

    protected virtual T NewOption(IContext context, NewOption e)
        => Default(context, e);

    protected virtual T NewOption(IHistoricContext historicContext, NewOption e)
        => NewOption(historicContext.Current, e);

    protected virtual T UpdateFractions(IContext context, UpdateFractions e)
        => Default(context, e);

    protected virtual T UpdateFractions(IHistoricContext historicContext, UpdateFractions e)
        => UpdateFractions( historicContext.Current, e);

    protected virtual T NewCharity(IContext context, NewCharity e)
        => Default(context, e);

    protected virtual T NewCharity(IHistoricContext historicContext, NewCharity e)
        => NewCharity(historicContext.Current, e);

    protected virtual T UpdateCharity(IContext context, UpdateCharity e)
        => Default(context, e);

    protected virtual T UpdateCharity( IHistoricContext historicContext, UpdateCharity e)
        => UpdateCharity(historicContext.Current, e);

    protected virtual T CharityPartition(IContext context, CharityPartition e)
        => Default(context, e);

    protected virtual T CharityPartition(IHistoricContext historicContext, CharityPartition e)
        => CharityPartition(historicContext.Current, e);

    protected virtual T NewDonation(IContext context, NewDonation e)
        => Default( context, e);

    protected virtual T NewDonation(IHistoricContext historicContext, NewDonation e)
        => NewDonation(historicContext.Current, e);

    protected virtual T UpdateCharityForDonation( IContext context, UpdateCharityForDonation e)
        => Default( context, e);

    protected virtual T UpdateCharityForDonation(IHistoricContext historicContext, UpdateCharityForDonation e)
        => UpdateCharityForDonation( historicContext.Current, e);

    protected virtual T CancelDonation(IContext context, CancelDonation e)
        => Default(context, e);

    protected virtual T CancelDonation( IHistoricContext historicContext, CancelDonation e)
        => CancelDonation( historicContext.Current, e);

    protected virtual T ConvLiquidate(IContext context, ConvLiquidate e)
        => Default(context, e);

    protected virtual T ConvLiquidate( IHistoricContext historicContext, ConvLiquidate e)
        => ConvLiquidate( historicContext.Current, e);

    protected virtual T ConvExit( IContext context, ConvExit e)
        => Default(context, e);

    protected virtual T ConvExit( IHistoricContext historicContext, ConvExit e)
        => ConvExit(historicContext.Current, e);

    protected virtual T ConvTransfer( IContext context, ConvTransfer e)
        => Default( context, e);

    protected virtual T ConvTransfer( IHistoricContext historicContext, ConvTransfer e)
        => ConvTransfer(historicContext.Current, e);

    protected virtual T ConvEnter(IContext context, ConvEnter e)
        => Default( context, e);

    protected virtual T ConvEnter(IHistoricContext historicContext, ConvEnter e)
        => ConvEnter(historicContext.Current, e);

    protected virtual T ConvInvest(IContext context, ConvInvest e)
        => Default( context, e);

    protected virtual T ConvInvest(IHistoricContext historicContext, ConvInvest e)
        => ConvInvest( historicContext.Current, e);

    protected virtual T IncreaseCash(IContext context, IncreaseCash e)
        => Default(context, e);

    protected virtual T IncreaseCash(IHistoricContext historicContext, IncreaseCash e)
        => IncreaseCash( historicContext.Current, e);

    protected virtual T Audit( IContext context, Audit e)
        => Default(context, e);

    protected virtual T Audit(IHistoricContext historicContext, Audit e)
        => Audit(historicContext.Current, e);

    protected virtual T Default(IContext context, Event e)
        => Start;
}
