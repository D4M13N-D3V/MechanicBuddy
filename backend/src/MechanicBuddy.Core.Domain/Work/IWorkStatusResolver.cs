namespace Carmasters.Core.Domain
{
    public interface IWorkStatusResolver 
    {
        WorkStatus Resolve(int workId);
    }
}