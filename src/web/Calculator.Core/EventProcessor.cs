namespace FfAdmin.Calculator.Core;

public class EventProcessor<T> : IEventProcessor<T>
    where T : class, IModel<T>
{
    object IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => Process((T)model, previousContext, context, e);

    public T Start => T.Empty;

    public virtual T Process(T model, IContext previousContext, IContext context, Event e)
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
        protected C GetCurrent<C>(IContext<C> context)
            where C : class
            => context.GetValue(currentContext);

        protected C GetPrevious<C>(IContext<C> context)
            where C : class
            => context.GetValue(previousContext);

        public virtual T Process(T model, Event e)
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
                IncreaseCash ic => IncreaseCash(model, ic),
                Audit a => Audit(model, a),
                PriceInfo pi => PriceInfo(model, pi),
                _ => model
            };

        protected virtual T Default(T model, Event e)
            => model;

        protected virtual T NewOption(T model, NewOption e)
            => Default(model, e);

        protected virtual T UpdateFractions(T model, UpdateFractions e)
            => Default(model, e);

        protected virtual T NewCharity(T model, NewCharity e)
            => Default(model, e);

        protected virtual T UpdateCharity(T model, UpdateCharity e)
            => Default(model, e);

        protected virtual T CharityPartition(T model, CharityPartition e)
            => Default(model, e);

        protected virtual T NewDonation(T model, NewDonation e)
            => Default(model, e);

        protected virtual T UpdateCharityForDonation(T model, UpdateCharityForDonation e)
            => Default(model, e);

        protected virtual T CancelDonation(T model, CancelDonation e)
            => Default(model, e);

        protected virtual T ConvLiquidate(T model, ConvLiquidate e)
            => Default(model, e);

        protected virtual T ConvExit(T model, ConvExit e)
            => Default(model, e);

        protected virtual T ConvTransfer(T model, ConvTransfer e)
            => Default(model, e);

        protected virtual T ConvEnter(T model, ConvEnter e)
            => Default(model, e);

        protected virtual T ConvInvest(T model, ConvInvest e)
            => Default(model, e);

        protected virtual T IncreaseCash(T model, IncreaseCash e)
            => Default(model, e);

        protected virtual T Audit(T model, Audit e)
            => Default(model, e);

        protected virtual T PriceInfo(T model, PriceInfo e)
            => Default(model, e);
    }

    public virtual IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();
}
