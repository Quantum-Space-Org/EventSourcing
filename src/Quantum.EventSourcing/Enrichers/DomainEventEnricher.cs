using System.Collections.Generic;
using System.Threading.Tasks;
using Quantum.UnitTests.EventSourcing;

namespace Quantum.EventSourcing.Enrichers;

public class DomainEventEnricher : IDomainEventEnricher
{
    private readonly ICollection<IDomainEventEnricher> _enrichers = new List<IDomainEventEnricher>()
    {
        new CorrelationIdEnricher(null),
        new UserEnricher(),
        new ServiceEnricher()
    };

    public Task<IsADomainEvent> Enrich(IsADomainEvent @event)
    {
        foreach (var domainEventEnricher in _enrichers)
        {
            domainEventEnricher.Enrich(@event);
        }
        return Task.FromResult(@event);
    }
}