namespace FfAdmin.InMemoryDatabase;

public abstract class EventProcessor<T> : IEventProcessor, IApplyToTypedDictionary
    where T : class
{
    object IEventProcessor.Start => Start;

    object IEventProcessor.Process(object model, IContext context, Event e)
        => Process((T)model, context, e);
    
    IApplyToTypedDictionary IEventProcessor.GetTypedDictionaryApplicator()
        => this;
    public abstract T Start { get; }

    public virtual T Process(T model, IContext context, Event e)
        => e switch
        {
            NewOption no => NewOption(model, context, no),
            UpdateFractions uf => UpdateFractions(model, context, uf),
            NewCharity nc => NewCharity(model, context, nc),
            UpdateCharity uc => UpdateCharity(model, context, uc),
            CharityPartition cp => CharityPartition(model, context, cp),
            NewDonation nd => NewDonation(model, context, nd),
            UpdateCharityForDonation ucd => UpdateCharityForDonation(model, context, ucd),
            CancelDonation cd => CancelDonation(model, context, cd),
            ConvLiquidate cl => ConvLiquidate(model, context, cl),
            ConvExit ce => ConvExit(model, context, ce),
            ConvTransfer ct => ConvTransfer(model, context, ct),
            ConvEnter ce => ConvEnter(model, context, ce),
            ConvInvest ci => ConvInvest(model, context, ci),
            IncreaseCash ic => IncreaseCash(model, context, ic),
            Audit a => Audit(model, context, a),
            _ => model
        };

    protected virtual T NewOption(T model, IContext context, NewOption e)
        => Default(model, context, e);

    protected virtual T UpdateFractions(T model, IContext context, UpdateFractions e)
        => Default(model, context, e);

    protected virtual T NewCharity(T model, IContext context, NewCharity e)
        => Default(model, context, e);

    protected virtual T UpdateCharity(T model, IContext context, UpdateCharity e)
        => Default(model, context, e);

    protected virtual T CharityPartition(T model, IContext context, CharityPartition e)
        => Default(model, context, e);

    protected virtual T NewDonation(T model, IContext context, NewDonation e)
        => Default(model, context, e);

    protected virtual T UpdateCharityForDonation(T model, IContext context, UpdateCharityForDonation e)
        => Default(model, context, e);

    protected virtual T CancelDonation(T model, IContext context, CancelDonation e)
        => Default(model, context, e);

    protected virtual T ConvLiquidate(T model, IContext context, ConvLiquidate e)
        => Default(model, context, e);

    protected virtual T ConvExit(T model, IContext context, ConvExit e)
        => Default(model, context, e);

    protected virtual T ConvTransfer(T model, IContext context, ConvTransfer e)
        => Default(model, context, e);

    protected virtual T ConvEnter(T model, IContext context, ConvEnter e)
        => Default(model, context, e);

    protected virtual T ConvInvest(T model, IContext context, ConvInvest e)
        => Default(model, context, e);

    protected virtual T IncreaseCash(T model, IContext context, IncreaseCash e)
        => Default(model, context, e);

    protected virtual T Audit(T model, IContext context, Audit e)
        => Default(model, context, e);

    protected virtual T Default(T model, IContext context, Event e)
        => model;


    TypedDictionary IApplyToTypedDictionary.Start(TypedDictionary context)
        => context.Set(() => Start);

    TypedDictionary IApplyToTypedDictionary.Process(TypedDictionary result, Func<TypedDictionary> context,
        IContext previousContext, Event e)
        => result.Set(() => Process(previousContext.GetContext<T>()!, context(), e));
}

