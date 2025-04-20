using Quantum.EventSourcing;

namespace Quantum.UnitTests.EventSourcing
{
    public class EventViewModelFactory
    {
        public static EventViewModel CreateEventViewModel(IsADomainEvent domainEvent,  AppendEventDto appendEventDto )
        {
            return new()
            {
                Version = 1,
                Payload = domainEvent,
                EventType = domainEvent.GetType().AssemblyQualifiedName,
                GlobalCommitPosition = 0,
                Metadata = appendEventDto?.Metadata,
                EventId = appendEventDto?.GlobalUniqueEventId
            };
        }
    }
}