using System.Collections.Generic;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing;

public class StreamWasMigratedFromDomainEvent : IsADomainEvent
{
    public string From { get; }

    public StreamWasMigratedFromDomainEvent(string aggregateId, string from) : base(aggregateId)
        => From = from;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return AggregateId;
    }
}