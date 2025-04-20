using System.Threading.Tasks;
using Quantum.UnitTests.EventSourcing;

namespace Quantum.EventSourcing.Enrichers;

public class UserEnricher : IDomainEventEnricher
{
    public Task<IsADomainEvent> Enrich(IsADomainEvent @event)
    {
        @event.MessageMetadata.CreatedBy = "Guest";
        return Task.FromResult(@event);
    }
}