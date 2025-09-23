using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public interface IModelCalculator<M, P>
{
    ValueTask<M> Calculate(IContext context, P parameter);
}

public static class ModelCalculatorExt
{
    public static ValueTask<M> Calculate<M>(this IModelCalculator<M, Unit> calculator, IContext context)
        => calculator.Calculate(context, Unit.Value);
} 
