using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public interface IModel<T>
    where T : class
{
    static abstract T Empty { get; } 
    static abstract IEventProcessor<T> Processor { get; }
}
