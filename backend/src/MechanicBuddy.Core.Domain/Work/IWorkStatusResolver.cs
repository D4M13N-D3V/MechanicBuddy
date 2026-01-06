namespace MechanicBuddy.Core.Domain
{
    public interface IWorkStatusResolver 
    {
        WorkStatus Resolve(int workId);
    }
}