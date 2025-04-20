using System.Collections.Generic;
using System.Linq;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing;

public class EventStreamViewModel
{
    public ICollection<EventViewModel> Events { get; }
    public ICollection<IsADomainEvent> Payloads
        => Events?.Select(e => e.Payload).ToList();

    public int Version { get; }
    public bool HasAnyEventYet { get; }

    public long Count => Events.Count;

    public EventStreamViewModel(ICollection<EventViewModel> events
        , int version
        , bool hasAnyEventYet)
    {

        Events = events;
        Version = version;
        HasAnyEventYet = hasAnyEventYet;
    }
}