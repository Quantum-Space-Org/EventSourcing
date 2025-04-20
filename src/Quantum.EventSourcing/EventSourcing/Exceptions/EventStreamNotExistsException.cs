using System;

namespace Quantum.EventSourcing;

public class EventStreamNotExistsException : Exception
{
    public EventStreamNotExistsException(string eventStreamId)
    {
        EventStreamId = eventStreamId;
    }

    public  string EventStreamId { get; }
}