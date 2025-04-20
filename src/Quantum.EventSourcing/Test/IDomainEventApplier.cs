using System.Collections.Generic;
using System.Threading.Tasks;
using Quantum.Domain;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing.Test
{
    public interface IDomainEventApplier
    {
        Task ApplyEventsToAndProject<T>(T eventStreamId, List<IsADomainEvent> @events) where T : IsAnIdentity;
        Task ApplyEventsTo<T>(T eventStreamId, List<IsADomainEvent> @events) where T : IsAnIdentity;
        Task ProjectEvents(List<IsADomainEvent> @events);

        Task ApplyEventToAndProject<T>(T eventStreamId, IsADomainEvent @event) where T : IsAnIdentity;
        Task ApplyEventTo<T>(T eventStreamId, IsADomainEvent @event) where T : IsAnIdentity;
        Task ProjectEvent(IsADomainEvent @event);
    }
}