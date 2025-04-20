namespace Quantum.EventSourcing;

public class Position
{
    public static EventStreamPositions At(int position) => (EventStreamPositions)position;
    public static EventStreamPositions AtStart() => EventStreamPositions.FromStart;
}