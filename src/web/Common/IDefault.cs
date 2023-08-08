namespace FfAdmin.Common;

public interface IDefault
{
    static abstract object Default { get; }
}

public interface IDefault<T> : IDefault
    where T : IDefault<T>
{
    static object IDefault.Default => T.Default;

    static new abstract T Default { get; }
}
