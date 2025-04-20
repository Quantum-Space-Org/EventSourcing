using System.Collections.Generic;
using Newtonsoft.Json;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing;

public class StreamWasMovedToEvent : IsADomainEvent
{

    public string To { get; set; }
    public string From { get; set; }

    [JsonConstructor]
    public StreamWasMovedToEvent(string aggregateId, string from, string to) : base(aggregateId)
    {
        From = from;
        To = to;
    }
    
    public StreamWasMovedToEvent(string from, string to) : this(from, from, to)
    {
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}