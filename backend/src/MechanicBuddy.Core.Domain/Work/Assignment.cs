using System;
using System.Collections.Generic;

namespace Carmasters.Core.Domain
{
    public class Assignment
    {
       
        protected Assignment() { }

        public Assignment(Work work, Employee mechanic)
        {
            this.work = work ?? throw new ArgumentNullException(nameof(work));
            this.Mechanic = mechanic ?? throw new ArgumentNullException(nameof(mechanic));
        }
        private readonly Work work;
        public virtual Employee Mechanic { get; protected set; }

        public override bool Equals(object obj)
        {
            return obj is Assignment assignment &&
                   EqualityComparer<Work>.Default.Equals(work, assignment.work) &&
                   EqualityComparer<Employee>.Default.Equals(Mechanic, assignment.Mechanic);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(work, Mechanic);
        }
    }
}