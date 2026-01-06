using System;
using Carmasters.Core.Domain;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Carmasters.Core.Persistence.Postgres")]

namespace Carmasters.Core
{
     
    /// <summary>
    /// Integer identity base for entities.
    /// </summary>
    public class GuidIdentityEntity : IEntity<Guid> 
    {
        /// <summary>
        /// Gets a value indicating whether this instance is transient.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is transient; otherwise, <c>false</c>.
        /// </value>
        /// 

        public  virtual Guid Id { get; internal protected set; }

		bool IEntity<Guid>.IsTransient => Id == Guid.Empty;
         
         
        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            return Equals(obj as IEntity<Guid>);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public  virtual bool Equals(IEntity<Guid> obj)
        {
            return EntityEquals(obj as IEntity<Guid>);
        }


        protected bool EntityEquals(IEntity<Guid> other)
        {
            if (other == null || !GetType().IsInstanceOfType(other))
            {
                return false;
            }
            // One entity is transient and the other is persistent.
            else if (((IEntity<Guid>)this).IsTransient ^ other.IsTransient)
            {
                return false;
            }
            // Both entities are not saved.
            else if (((IEntity<Guid>)this).IsTransient && other.IsTransient)
            {
                return ReferenceEquals(this, other);
            }
            else
            {
                // Compare transient instances.
                return this.Id  == other.Id;
            }
        }


        public static bool operator ==(GuidIdentityEntity x, GuidIdentityEntity y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(GuidIdentityEntity x, GuidIdentityEntity y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            cachedHashCode = ((IEntity<Guid>)this).IsTransient ? base.GetHashCode() : Id.GetHashCode();
            return cachedHashCode.Value;
        }

    }

    /// <summary>
    /// Marker interface for entities.
    /// </summary>
    /// <typeparam name="TIdentity">The type of the identity.</typeparam>
    public  interface IEntity<TIdentity>
    {
        /// <summary>
        /// Gets the entity's identity.
        /// </summary>
        /// <value>The identity.</value>
        public  TIdentity Id { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is transient.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is transient; otherwise, <c>false</c>.
        /// </value>
        protected internal bool IsTransient { get; }
    }
}
