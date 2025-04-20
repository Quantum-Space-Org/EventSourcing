namespace Quantum.EventSourcing;

public class EventStreamMetaData
{
    public int Version { get;  set; }

    public EventStreamPositions Positions { get;  set; }

    public bool MasrkAsDeleted { get;  set; }
}