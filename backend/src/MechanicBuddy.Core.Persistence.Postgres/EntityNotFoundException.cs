using System;

namespace MechanicBuddy.Core.Repository.Postgres
{ 
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string name) : base($"enitity '{name}' not found")
        {
        }
    }
}