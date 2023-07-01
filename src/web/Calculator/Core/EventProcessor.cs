namespace FfAdmin.Calculator.Core;

public abstract class EventProcessor<T> : IEventProcessor<T>
    where T : class
{
    object IEventProcessor.Start => Start;

    object IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => Process((T)model, previousContext, context, e);

    public abstract T Start { get; }
    public Type ModelType => typeof(T);
    public virtual IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();

    public virtual T Process(T model, IContext previousContext, IContext context, Event e)
        => e switch
        {
            NewOption no => NewOption(model, previousContext, context, no),
            UpdateFractions uf => UpdateFractions(model, previousContext, context, uf),
            NewCharity nc => NewCharity(model, previousContext, context, nc),
            UpdateCharity uc => UpdateCharity(model, previousContext, context, uc),
            CharityPartition cp => CharityPartition(model, previousContext, context, cp),
            NewDonation nd => NewDonation(model, previousContext, context, nd),
            UpdateCharityForDonation ucd => UpdateCharityForDonation(model, previousContext, context, ucd),
            CancelDonation cd => CancelDonation(model, previousContext, context, cd),
            ConvLiquidate cl => ConvLiquidate(model, previousContext, context, cl),
            ConvExit ce => ConvExit(model, previousContext, context, ce),
            ConvTransfer ct => ConvTransfer(model, previousContext, context, ct),
            ConvEnter ce => ConvEnter(model, previousContext, context, ce),
            ConvInvest ci => ConvInvest(model, previousContext, context, ci),
            IncreaseCash ic => IncreaseCash(model, previousContext, context, ic),
            Audit a => Audit(model, previousContext, context, a),
            _ => model
        };

    protected virtual T NewOption(T model, IContext context, NewOption e)
        => Default(model, context, e);

    protected virtual T NewOption(T model, IContext previousContext, IContext context, NewOption e)
        => NewOption(model, context, e);

    protected virtual T UpdateFractions(T model, IContext context, UpdateFractions e)
        => Default(model, context, e);

    protected virtual T UpdateFractions(T model, IContext previousContext, IContext context, UpdateFractions e)
        => UpdateFractions(model, context, e);

    protected virtual T NewCharity(T model, IContext context, NewCharity e)
        => Default(model, context, e);

    protected virtual T NewCharity(T model, IContext previousContext, IContext context, NewCharity e)
        => NewCharity(model, context, e);

    protected virtual T UpdateCharity(T model, IContext context, UpdateCharity e)
        => Default(model, context, e);

    protected virtual T UpdateCharity(T model, IContext previousContext, IContext context, UpdateCharity e)
        => UpdateCharity(model, context, e);

    protected virtual T CharityPartition(T model, IContext context, CharityPartition e)
        => Default(model, context, e);

    protected virtual T CharityPartition(T model, IContext previousContext, IContext context, CharityPartition e)
        => CharityPartition(model, context, e);

    protected virtual T NewDonation(T model, IContext context, NewDonation e)
        => Default(model, context, e);

    protected virtual T NewDonation(T model, IContext previousContext, IContext context, NewDonation e)
        => NewDonation(model, context, e);

    protected virtual T UpdateCharityForDonation(T model, IContext context, UpdateCharityForDonation e)
        => Default(model, context, e);

    protected virtual T UpdateCharityForDonation(T model, IContext previousContext, IContext context,
        UpdateCharityForDonation e)
        => UpdateCharityForDonation(model, context, e);

    protected virtual T CancelDonation(T model, IContext context, CancelDonation e)
        => Default(model, context, e);

    protected virtual T CancelDonation(T model, IContext previousContext, IContext context, CancelDonation e)
        => CancelDonation(model, context, e);

    protected virtual T ConvLiquidate(T model, IContext context, ConvLiquidate e)
        => Default(model, context, e);

    protected virtual T ConvLiquidate(T model, IContext previousContext, IContext context, ConvLiquidate e)
        => ConvLiquidate(model, context, e);

    protected virtual T ConvExit(T model, IContext context, ConvExit e)
        => Default(model, context, e);

    protected virtual T ConvExit(T model, IContext previousContext, IContext context, ConvExit e)
        => ConvExit(model, context, e);

    protected virtual T ConvTransfer(T model, IContext context, ConvTransfer e)
        => Default(model, context, e);

    protected virtual T ConvTransfer(T model, IContext previousContext, IContext context, ConvTransfer e)
        => ConvTransfer(model, context, e);

    protected virtual T ConvEnter(T model, IContext context, ConvEnter e)
        => Default(model, context, e);

    protected virtual T ConvEnter(T model, IContext previousContext, IContext context, ConvEnter e)
        => ConvEnter(model, context, e);

    protected virtual T ConvInvest(T model, IContext context, ConvInvest e)
        => Default(model, context, e);

    protected virtual T ConvInvest(T model, IContext previousContext, IContext context, ConvInvest e)
        => ConvInvest(model, context, e);

    protected virtual T IncreaseCash(T model, IContext context, IncreaseCash e)
        => Default(model, context, e);

    protected virtual T IncreaseCash(T model, IContext previousContext, IContext context, IncreaseCash e)
        => IncreaseCash(model, context, e);

    protected virtual T Audit(T model, IContext context, Audit e)
        => Default(model, context, e);

    protected virtual T Audit(T model, IContext previousContext, IContext context, Audit e)
        => Audit(model, context, e);

    protected virtual T Default(T model, IContext context, Event e)
        => model;
}
