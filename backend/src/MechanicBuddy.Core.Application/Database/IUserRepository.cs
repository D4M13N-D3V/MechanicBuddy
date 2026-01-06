using Carmasters.Core.Application.Model;
using Carmasters.Core.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net.Mail;

namespace Carmasters.Core.Application.Database
{
    public interface IUserRepository
    {
        public User GetBy(string userName);
        public User GetByEmail(string email);
        public User GetBy(UserIdentifier id); 
        void Update(User user);
        string GetFullName(string userName);
        IEnumerable<User> GetAllByTenant(string tenantName);
    }
}