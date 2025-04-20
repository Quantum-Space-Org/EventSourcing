using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quantum.DataBase;
using Quantum.DataBase.EntityFramework;
using Quantum.Domain;
using Quantum.Domain.Messages.Event;
using Quantum.EventSourcing.Projection;
using Quantum.Resolver;

namespace Quantum.EventSourcing.Test
{
    public class DomainEventApplier(
        ILedger ledger,
        IEventStore eventStore,
        IResolver resolver,
        QuantumDbContext quantumDbContext)
        : IDomainEventApplier
    {
        public async Task ApplyEventToAndProject<T>(T eventStreamId, IsADomainEvent @event) where T : IsAnIdentity
        {
            await ApplyEventTo(eventStreamId, @event);
            await ProjectEvent(@event);

            

        }
        public async Task ApplyEventTo<T>(T eventStreamId, IsADomainEvent @event) where T : IsAnIdentity
        {
            await eventStore.AppendToEventStreamAsync(eventStreamId, AppendEventDto.Version1(@event));
        }
        public async Task ProjectEvent(IsADomainEvent @event)
        {
            var assemblyQualifiedName = @event.GetType().AssemblyQualifiedName;
            var projectorTypes = ledger.WhoAreInterestedIn(Type.GetType(assemblyQualifiedName));
            foreach (var projectorType in projectorTypes)
            {
                var projector = resolver.Resolve(projectorType);
                await ((ImAProjector)projector).Process(@event);
            }

            await quantumDbContext.SaveChangesAsync();
        }

        public async Task ApplyEventsToAndProject<T>(T eventStreamId, List<IsADomainEvent> events) where T : IsAnIdentity
        {
            await ApplyEventsTo(eventStreamId, events);
            await ProjectEvents(events);
        }

        public async Task ApplyEventsTo<T>(T eventStreamId, List<IsADomainEvent> events) where T : IsAnIdentity
        {
            await eventStore.AppendToEventStreamAsync(eventStreamId, events.Select(AppendEventDto.Version1).ToList());
        }

        public async Task ProjectEvents(List<IsADomainEvent> events)
        {
            foreach (var @event in events)
            {
                await ProjectEvent(@event);
            }
        }
    }
}