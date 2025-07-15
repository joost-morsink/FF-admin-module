namespace FfAdmin.Calculator.Core;

public abstract class ContextualCalculator<T> : IContextualCalculator<T>
    where T : class, IModel<T>
{
    public Type ModelType => typeof(T);
    public virtual IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();
    
    public virtual ValueTask<T> Process(IContext previousContext, IContext context, Event e)
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
        protected ValueTask<C> GetCurrent<C>(IContext<C> context)
            where C : class
            => context.GetValue(currentContext);

        protected ValueTask<C> GetPrevious<C>(IContext<C> context)
            where C : class
            => context.GetValue(previousContext);

        public virtual ValueTask<T> Process(Event e)
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
                ConvInflation ci => ConvInflation(ci),
                IncreaseCash ic => IncreaseCash(ic),
                Audit a => Audit(a),
                PriceInfo pi => PriceInfo(pi),
                _ => Default(e)
            };

        protected virtual ValueTask<T> NewOption(NewOption e)
            => Default(e);

        protected virtual ValueTask<T> UpdateFractions(UpdateFractions e)
            => Default(e);

        protected virtual ValueTask<T> NewCharity(NewCharity e)
            => Default(e);

        protected virtual ValueTask<T> UpdateCharity(UpdateCharity e)
            => Default(e);

        protected virtual ValueTask<T> CharityPartition(CharityPartition e)
            => Default(e);

        protected virtual ValueTask<T> NewDonation(NewDonation e)
            => Default(e);

        protected virtual ValueTask<T> UpdateCharityForDonation(UpdateCharityForDonation e)
            => Default(e);

        protected virtual ValueTask<T> CancelDonation(CancelDonation e)
            => Default(e);

        protected virtual ValueTask<T> ConvLiquidate(ConvLiquidate e)
            => Default(e);

        protected virtual ValueTask<T> ConvExit(ConvExit e)
            => Default(e);

        protected virtual ValueTask<T> ConvTransfer(ConvTransfer e)
            => Default(e);

        protected virtual ValueTask<T> ConvEnter(ConvEnter e)
            => Default(e);

        protected virtual ValueTask<T> ConvInvest(ConvInvest e)
            => Default(e);

        protected virtual ValueTask<T> ConvInflation(ConvInflation e)
            => Default(e);
        
        protected virtual ValueTask<T> IncreaseCash(IncreaseCash e)
            => Default(e);

        protected virtual ValueTask<T> Audit(Audit e)
            => Default(e);

        protected virtual ValueTask<T> PriceInfo(PriceInfo e)
            => Default(e);

        protected virtual ValueTask<T> Default(Event e)
            => new(T.GetMetaModel().Empty);
    }
}
