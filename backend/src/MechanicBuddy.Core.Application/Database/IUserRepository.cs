using MechanicBuddy.Core.Application.Model;
using MechanicBuddy.Core.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net.Mail;

namespace MechanicBuddy.Core.Application.Database
{
    public interface IUserRepository
    {
        public User GetBy(string userName);
        public User GetByEmail(string email);
        public User GetBy(UserIdentifier id); 
        void Add(User user);
        void Update(User user);
        string GetFullName(string userName);
        IEnumerable<User> GetAllByTenant(string tenantName);
    }
}