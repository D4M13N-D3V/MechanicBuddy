namespace MechanicBuddy.Core.Domain
{
    public interface ISequnceNumberProviderFactory
    {
        ISequencedNumberProvider GetNumberProvider<T>();
    }
}