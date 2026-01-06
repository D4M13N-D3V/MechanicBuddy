using AutoMapper;
using Carmasters.Core.Application.Model;
using Carmasters.Core.Domain;
using NHibernate.Bytecode;
using System;
using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Carmasters.Core.Persistence.Postgres")]

namespace Carmasters.Core.Application.Model
{
}
namespace Carmasters.Core.Application
{
    public class User
    { 
        protected User() { }
        public User(string userName, string password, string email, bool validated, byte[] profileImage,  UserIdentifier id = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException($"'{nameof(userName)}' cannot be null or whitespace", nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"'{nameof(password)}' cannot be null or whitespace", nameof(password));
            }
            Email = email;
            UserName = userName;
            Password = password;
            Validated = validated;
            ProfileImage = profileImage;
            Id = id;
        }

        public virtual byte[] ProfileImage { get; protected set; }
        public virtual string Email { get; protected set; }

        public virtual bool Validated { get;protected set; }
        public virtual string UserName { get; protected set; }
        public virtual string Password { get;protected set; }
        public virtual UserIdentifier Id { get; protected internal set; }

        public override bool Equals(object obj)
        {
            return obj is User user &&
                   Id == user.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public virtual void ChangeEmail(string email)
        {
            //get validation email?
            this.Email = email;
        }

        public virtual void ChangeProfileImage(byte[] profileImage)
        {

            const int fiveMb = 5 * 1024 * 1024; 
            var fileSize = profileImage == null?0: profileImage.Length;
            if (fileSize > fiveMb)
            {
                throw new UserException("Profile image is too big");
            }

            this.ProfileImage = profileImage;
        }

        public virtual void ChangePassword(string v)
        {
            this.Password = v;
        }

        public virtual void ChangeUserName(string userName)
        {
            this.UserName = userName;
        }
    }
}