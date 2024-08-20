namespace FfAdmin.Calculator.Core;

public abstract class ContextualCalculator<T> : IContextualCalculator<T>
    where T : class, IModel<T>
{
    object IEventProcessor.Start => Start;

    object IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => Process(previousContext, context, e);

    public T Start => T.Empty;
    public Type ModelType => typeof(T);
    public virtual IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();
    
    public virtual T Process(IContext previousContext, IContext context, Event e)
    {
        var calculation = GetCalculation(previousContext, context);
        
        return calculation.Process(e);
    }

    protected virtual BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
    {
        return new BaseCalculation(previousContext, currentContext);
    }

    protected class BaseCalculation(IContext previousContext, IContext currentContext)
    {
        protected C GetCurrent<C>(IContext<C> context)
            where C : class
            => context.GetValue(currentContext);

        protected C GetPrevious<C>(IContext<C> context)
            where C : class
            => context.GetValue(previousContext);

        public virtual T Process(Event e)
            => e switch
            {
                NewOption no => NewOption(no),
                UpdateFractions uf => UpdateFractions(uf),
                NewCharity nc => NewCharity(nc),
                UpdateCharity uc => UpdateCharity(uc),
                CharityPartition cp => CharityPartition(cp),
                NewDonation nd => NewDonation(nd),
                UpdateCharityForDonation ucd => UpdateCharityForDonation(ucd),
                CancelDonation cd => CancelDonation(cd),
                ConvLiquidate cl => ConvLiquidate(cl),
                ConvExit ce => ConvExit(ce),
                ConvTransfer ct => ConvTransfer(ct),
                ConvEnter ce => ConvEnter(ce),
                ConvInvest ci => ConvInvest(ci),
                IncreaseCash ic => IncreaseCash(ic),
                Audit a => Audit(a),
                PriceInfo pi => PriceInfo(pi),
                _ => Default(e)
            };

        protected virtual T NewOption(NewOption e)
            => Default(e);

        protected virtual T UpdateFractions(UpdateFractions e)
            => Default(e);

        protected virtual T NewCharity(NewCharity e)
            => Default(e);

        protected virtual T UpdateCharity(UpdateCharity e)
            => Default(e);

        protected virtual T CharityPartition(CharityPartition e)
            => Default(e);

        protected virtual T NewDonation(NewDonation e)
            => Default(e);

        protected virtual T UpdateCharityForDonation(UpdateCharityForDonation e)
            => Default(e);

        protected virtual T CancelDonation(CancelDonation e)
            => Default(e);

        protected virtual T ConvLiquidate(ConvLiquidate e)
            => Default(e);

        protected virtual T ConvExit(ConvExit e)
            => Default(e);

        protected virtual T ConvTransfer(ConvTransfer e)
            => Default(e);

        protected virtual T ConvEnter(ConvEnter e)
            => Default(e);

        protected virtual T ConvInvest(ConvInvest e)
            => Default(e);

        protected virtual T IncreaseCash(IncreaseCash e)
            => Default(e);

        protected virtual T Audit(Audit e)
            => Default(e);

        protected virtual T PriceInfo(PriceInfo e)
            => Default(e);

        protected virtual T Default(Event e)
            => T.Empty;
    }
}
