using System;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule;

public interface IMutableContext<T> 
{
    T Value { get; set; }

    async Task<R> With<R>(T value, Func<Task<R>> action)
    {
        var old = Value;
        Value = value;
        try
        {
            return await action();
        }
        finally
        {
            Value = old;
        }
    }

    Task With(T value, Func<Task> action)
        => With(value, async () =>
        {
            await action();
            return 0;
        });

    R With<R>(T value, Func<R> action)
    {
        var old = Value;
        Value = value;
        try
        {
            return action();
        }
        finally
        {
            Value = old;
        }
    }

    void With(T value, Action action)
        => With(value, () =>
        {
            action();
            return 0;
        });
}