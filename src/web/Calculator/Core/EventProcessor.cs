namespace FfAdmin.Calculator.Core;

public abstract class EventProcessor<T> : IEventProcessor<T>
    where T : class
{
    object IEventProcessor.Start => Start;

    object IEventProcessor.Process(object model, IHistoricContext historicContext, Event e)
        => Process((T)model, historicContext, e);

    public abstract T Start { get; }
    public Type ModelType => typeof(T);

    public virtual T Process(T model, IHistoricContext historicContext, Event e)
        => e switch
        {
            NewOption no => NewOption(model, historicContext, no),
            UpdateFractions uf => UpdateFractions(model, historicContext, uf),
            NewCharity nc => NewCharity(model, historicContext, nc),
            UpdateCharity uc => UpdateCharity(model, historicContext, uc),
            CharityPartition cp => CharityPartition(model, historicContext, cp),
            NewDonation nd => NewDonation(model, historicContext, nd),
            UpdateCharityForDonation ucd => UpdateCharityForDonation(model, historicContext, ucd),
            CancelDonation cd => CancelDonation(model, historicContext, cd),
            ConvLiquidate cl => ConvLiquidate(model, historicContext, cl),
            ConvExit ce => ConvExit(model, historicContext, ce),
            ConvTransfer ct => ConvTransfer(model, historicContext, ct),
            ConvEnter ce => ConvEnter(model, historicContext, ce),
            ConvInvest ci => ConvInvest(model, historicContext, ci),
            IncreaseCash ic => IncreaseCash(model, historicContext, ic),
            Audit a => Audit(model, historicContext, a),
            _ => model
        };

    public virtual Func<int, T> PositionalModelCreator(EventStream stream)
        => position =>
        {
            if (position == 0)
                return Start;
            var historicContext = stream.GetHistoricContext(position);
            return Process(historicContext.Previous.GetContext<T>(), historicContext, stream.Events[position - 1]);
        };

    Delegate IEventProcessor.PositionalModelCreator(EventStream stream)
        => PositionalModelCreator(stream);

    protected virtual T NewOption(T model, IContext context, NewOption e)
        => Default(model, context, e);

    protected virtual T NewOption(T model, IHistoricContext historicContext, NewOption e)
        => NewOption(model, historicContext.Current, e);

    protected virtual T UpdateFractions(T model, IContext context, UpdateFractions e)
        => Default(model, context, e);

    protected virtual T UpdateFractions(T model, IHistoricContext historicContext, UpdateFractions e)
        => UpdateFractions(model, historicContext.Current, e);

    protected virtual T NewCharity(T model, IContext context, NewCharity e)
        => Default(model, context, e);

    protected virtual T NewCharity(T model, IHistoricContext historicContext, NewCharity e)
        => NewCharity(model, historicContext.Current, e);

    protected virtual T UpdateCharity(T model, IContext context, UpdateCharity e)
        => Default(model, context, e);

    protected virtual T UpdateCharity(T model, IHistoricContext historicContext, UpdateCharity e)
        => UpdateCharity(model, historicContext.Current, e);

    protected virtual T CharityPartition(T model, IContext context, CharityPartition e)
        => Default(model, context, e);

    protected virtual T CharityPartition(T model, IHistoricContext historicContext, CharityPartition e)
        => CharityPartition(model, historicContext.Current, e);

    protected virtual T NewDonation(T model, IContext context, NewDonation e)
        => Default(model, context, e);

    protected virtual T NewDonation(T model, IHistoricContext historicContext, NewDonation e)
        => NewDonation(model, historicContext.Current, e);

    protected virtual T UpdateCharityForDonation(T model, IContext context, UpdateCharityForDonation e)
        => Default(model, context, e);

    protected virtual T UpdateCharityForDonation(T model, IHistoricContext historicContext, UpdateCharityForDonation e)
        => UpdateCharityForDonation(model, historicContext.Current, e);

    protected virtual T CancelDonation(T model, IContext context, CancelDonation e)
        => Default(model, context, e);

    protected virtual T CancelDonation(T model, IHistoricContext historicContext, CancelDonation e)
        => CancelDonation(model, historicContext.Current, e);

    protected virtual T ConvLiquidate(T model, IContext context, ConvLiquidate e)
        => Default(model, context, e);

    protected virtual T ConvLiquidate(T model, IHistoricContext historicContext, ConvLiquidate e)
        => ConvLiquidate(model, historicContext.Current, e);

    protected virtual T ConvExit(T model, IContext context, ConvExit e)
        => Default(model, context, e);

    protected virtual T ConvExit(T model, IHistoricContext historicContext, ConvExit e)
        => ConvExit(model, historicContext.Current, e);

    protected virtual T ConvTransfer(T model, IContext context, ConvTransfer e)
        => Default(model, context, e);

    protected virtual T ConvTransfer(T model, IHistoricContext historicContext, ConvTransfer e)
        => ConvTransfer(model, historicContext.Current, e);

    protected virtual T ConvEnter(T model, IContext context, ConvEnter e)
        => Default(model, context, e);

    protected virtual T ConvEnter(T model, IHistoricContext historicContext, ConvEnter e)
        => ConvEnter(model, historicContext.Current, e);

    protected virtual T ConvInvest(T model, IContext context, ConvInvest e)
        => Default(model, context, e);

    protected virtual T ConvInvest(T model, IHistoricContext historicContext, ConvInvest e)
        => ConvInvest(model, historicContext.Current, e);

    protected virtual T IncreaseCash(T model, IContext context, IncreaseCash e)
        => Default(model, context, e);

    protected virtual T IncreaseCash(T model, IHistoricContext historicContext, IncreaseCash e)
        => IncreaseCash(model, historicContext.Current, e);

    protected virtual T Audit(T model, IContext context, Audit e)
        => Default(model, context, e);

    protected virtual T Audit(T model, IHistoricContext historicContext, Audit e)
        => Audit(model, historicContext.Current, e);

    protected virtual T Default(T model, IContext context, Event e)
        => model;
}
