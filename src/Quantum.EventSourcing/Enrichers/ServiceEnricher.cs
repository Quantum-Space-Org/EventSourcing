using System.Threading.Tasks;
using Quantum.UnitTests.EventSourcing;

namespace Quantum.EventSourcing.Enrichers;

public class ServiceEnricher : IDomainEventEnricher
{
    public Task<IsADomainEvent> Enrich(IsADomainEvent @event)
    {
        @event.MessageMetadata.ServiceName = "Accounting";
        return Task.FromResult(@event);
    }
}