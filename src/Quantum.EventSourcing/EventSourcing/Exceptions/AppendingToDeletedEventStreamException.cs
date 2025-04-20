using System;

namespace Quantum.EventSourcing.Exceptions;

public class AppendingToDeletedEventStreamException : Exception
{
    public string EventStreamId;

    public AppendingToDeletedEventStreamException(string eventStreamId)
    {
        EventStreamId = eventStreamId;
    }
}