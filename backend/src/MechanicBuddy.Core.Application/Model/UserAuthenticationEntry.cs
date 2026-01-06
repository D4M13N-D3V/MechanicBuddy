using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carmasters.Core.Application.Model
{
    public enum AuthenticationMethod
    {
        None, Password, IdCard, MobileId, Test, Google, Facebook
    }

    public class UserAuthenticationLog : IEquatable<UserAuthenticationLog>
    {

        public UserAuthenticationLog(User user, AuthenticationMethod authenticationMethod, DateTime authenticatedOn, string clientIp, int id = 0)
        {
            AuthenticationMethod = authenticationMethod;
            AuthenticatedOn = authenticatedOn;
            ClientIp = clientIp;
            User = user;
            Id = id;
        }
        public AuthenticationMethod AuthenticationMethod { get; }
        public DateTime AuthenticatedOn { get; }
        public string ClientIp { get; }
        public User User { get; }
        public int Id { get; internal set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserAuthenticationLog);
        }

        public bool Equals(UserAuthenticationLog other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(UserAuthenticationLog left, UserAuthenticationLog right)
        {
            return EqualityComparer<UserAuthenticationLog>.Default.Equals(left, right);
        }

        public static bool operator !=(UserAuthenticationLog left, UserAuthenticationLog right)
        {
            return !(left == right);
        }
    }
}
