using Carmasters.Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Criterion;
using System;

namespace Carmasters.Core.Repository.Postgres
{

    public class SequenceNumberProviderFactory : ISequnceNumberProviderFactory
    {
        private readonly IServiceProvider serviceProvider;

        public SequenceNumberProviderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public ISequencedNumberProvider GetNumberProvider<T>()
        {  
            if (typeof(T) == typeof(Invoice)) return serviceProvider.GetRequiredService<InvoiceSequenceNumberProvider>();
            if (typeof(T) == typeof(Work)) return serviceProvider.GetRequiredService<WorkSequenceNumberProvider>();
            if (typeof(T) == typeof(Estimate)) return serviceProvider.GetRequiredService<EstimateSequenceNumberProvider>();
            throw new NotSupportedException(typeof(T).Name);
        }
    }

    public class EstimateSequenceNumberProvider : ISequencedNumberProvider
    {
        private readonly ISession session;

        public EstimateSequenceNumberProvider(ISession session)
        {
            this.session = session;
        }
        public int Next()
        {
            var next = session.QueryOver<Estimate>()
                .Select(Projections.Max<Estimate>(x => x.Number))
                .UnderlyingCriteria.UniqueResult<int>();
            return next + 1;
        }
    }

    public class WorkSequenceNumberProvider : ISequencedNumberProvider
    {
        private readonly ISession session;

        public WorkSequenceNumberProvider(ISession session)
        {
            this.session = session;
        }
        public int Next()
        {
            var next = session.QueryOver<Work>()
                .Select(Projections.Max<Work>(x => x.Number))
                .UnderlyingCriteria.UniqueResult<int>();
            return next + 1;
        }
    }
    public class InvoiceSequenceNumberProvider : ISequencedNumberProvider
    {
        private readonly ISession session;

        public InvoiceSequenceNumberProvider(ISession session)
        {
            this.session = session;
        }
        public int Next()
        {
            var next = session.QueryOver<Invoice>()
                .Select(Projections.Max<Invoice>(x => x.Number))
                .UnderlyingCriteria.UniqueResult<int>();
            return next + 1;
        }
    } 
}