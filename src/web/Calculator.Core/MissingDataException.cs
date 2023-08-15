namespace FfAdmin.Calculator.Core;

public class MissingDataException : Exception
{
    public int Index { get; }
    public Type ModelType { get; }

    public MissingDataException(int index, Type modelType)
        : base($"Missing data for {modelType.Name} at position {index}")
    {
        Index = index;
        ModelType = modelType;
    }
}
