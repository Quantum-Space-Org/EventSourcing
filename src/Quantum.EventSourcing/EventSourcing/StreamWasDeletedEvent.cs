using System.Collections.Generic;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing;

public class StreamWasDeletedEvent : DeleteEvent
{
    public StreamWasDeletedEvent(string id) : base(id)
    {
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return AggregateId;
    }
}