using System.Threading.Tasks;
using Quantum.Domain.Messages.Event;

namespace Quantum.UnitTests.EventSourcing;

public interface IDomainEventEnricher
{
    Task<IsADomainEvent> Enrich(IsADomainEvent @event);
}