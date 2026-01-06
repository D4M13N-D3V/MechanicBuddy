using System;

namespace Carmasters.Core.Repository.Postgres
{ 
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string name) : base($"enitity '{name}' not found")
        {
        }
    }
}