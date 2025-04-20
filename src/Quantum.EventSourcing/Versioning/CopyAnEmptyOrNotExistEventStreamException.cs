using System;

namespace Quantum.EventSourcing.Versioning;

public class CopyAnEmptyOrNotExistEventStreamException : Exception
{
    public string StreamId { get; }

    public CopyAnEmptyOrNotExistEventStreamException(string streamId) : base($"{streamId} is empty or not exist!")
    {
        StreamId = streamId;
    }
}