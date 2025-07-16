namespace FfAdmin.Calculator.Core;

public class EventProcessor<T> : IEventProcessor<T>
    where T : class
{
    async ValueTask<object> IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => await Process((T)model, previousContext, context, e);

    public virtual ValueTask<T> Process(T model, IContext previousContext, IContext context, Event e)
    {
        var calculation = GetCalculation(previousContext, context);
        return calculation.Process(model, e);
    }

    protected virtual BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
    {
        var calculation = new BaseCalculation(previousContext, currentContext);
        return calculation;
    }

    protected class BaseCalculation(IContext previousContext, IContext currentContext)
    {
        protected ValueTask<C> GetCurrent<C>(IContext<C> context)
            where C : class
            => context.GetValue(currentContext);

        protected ValueTask<D> GetCurrent<H, K, D>(IContext<H, K, D> context, H header, K key)
            where H : class, IModel<H, K, D>
            where K : notnull
            where D : class
            => context.GetValue(currentContext, header, key);
        
        protected ValueTask<C> GetPrevious<C>(IContext<C> context)
            where C : class
            => context.GetValue(previousContext);

        protected ValueTask<D> GetPrevious<H, K, D>(IContext<H, K, D> context, H header, K key)
            where H : class, IModel<H, K, D>
            where K : notnull
            where D : class
            => context.GetValue(previousContext, header, key);

        public virtual ValueTask<T> Process(T model, Event e)
            => e switch
            {
                NewOption no => NewOption(model, no),
                UpdateFractions uf => UpdateFractions(model, uf),
                NewCharity nc => NewCharity(model, nc),
                UpdateCharity uc => UpdateCharity(model, uc),
                CharityPartition cp => CharityPartition(model, cp),
                NewDonation nd => NewDonation(model, nd),
                UpdateCharityForDonation ucd => UpdateCharityForDonation(model, ucd),
                CancelDonation cd => CancelDonation(model, cd),
                ConvLiquidate cl => ConvLiquidate(model, cl),
                ConvExit ce => ConvExit(model, ce),
                ConvTransfer ct => ConvTransfer(model, ct),
                ConvEnter ce => ConvEnter(model, ce),
                ConvInvest ci => ConvInvest(model, ci),
                ConvInflation ci => ConvInflation(model, ci),
                IncreaseCash ic => IncreaseCash(model, ic),
                Audit a => Audit(model, a),
                PriceInfo pi => PriceInfo(model, pi),
                _ => new(model)
            };

        protected virtual ValueTask<T> Default(T model, Event e)
            => new(model);

        protected virtual ValueTask<T> NewOption(T model, NewOption e)
            => Default(model, e);

        protected virtual ValueTask<T> UpdateFractions(T model, UpdateFractions e)
            => Default(model, e);

        protected virtual ValueTask<T> NewCharity(T model, NewCharity e)
            => Default(model, e);

        protected virtual ValueTask<T> UpdateCharity(T model, UpdateCharity e)
            => Default(model, e);

        protected virtual ValueTask<T> CharityPartition(T model, CharityPartition e)
            => Default(model, e);

        protected virtual ValueTask<T> NewDonation(T model, NewDonation e)
            => Default(model, e);

        protected virtual ValueTask<T> UpdateCharityForDonation(T model, UpdateCharityForDonation e)
            => Default(model, e);

        protected virtual ValueTask<T> CancelDonation(T model, CancelDonation e)
            => Default(model, e);

        protected virtual ValueTask<T> ConvLiquidate(T model, ConvLiquidate e)
            => Default(model, e);

        protected virtual ValueTask<T> ConvExit(T model, ConvExit e)
            => Default(model, e);

        protected virtual ValueTask<T> ConvTransfer(T model, ConvTransfer e)
            => Default(model, e);

        protected virtual ValueTask<T> ConvEnter(T model, ConvEnter e)
            => Default(model, e);

        protected virtual ValueTask<T> ConvInvest(T model, ConvInvest e)
            => Default(model, e);

        protected virtual ValueTask<T> ConvInflation(T model, ConvInflation e)
            => Default(model, e);
        
        protected virtual ValueTask<T> IncreaseCash(T model, IncreaseCash e)
            => Default(model, e);

        protected virtual ValueTask<T> Audit(T model, Audit e)
            => Default(model, e);

        protected virtual ValueTask<T> PriceInfo(T model, PriceInfo e)
            => Default(model, e);
    }

    public virtual IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();
}
