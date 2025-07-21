namespace FfAdmin.Calculator;

public class MetaModels
{
    public MetaModels(IEnumerable<IMetaModel> metaModels)
    {
        _metaModelDictionary = metaModels.ToImmutableDictionary(mm => mm.HeaderType);
    }

    private readonly ImmutableDictionary<Type, IMetaModel> _metaModelDictionary;

    public IMetaModel? Get(Type type)
        => _metaModelDictionary.GetValueOrDefault(type);
    public IEnumerable<Type> AvailableTypes
        => _metaModelDictionary.Keys;
}