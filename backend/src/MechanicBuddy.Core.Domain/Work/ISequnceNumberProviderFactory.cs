namespace Carmasters.Core.Domain
{
    public interface ISequnceNumberProviderFactory
    {
        ISequencedNumberProvider GetNumberProvider<T>();
    }
}