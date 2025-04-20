namespace Quantum.EventSourcing.Subscriber;

public interface ISubscriber
{
    string Name { get; }
    void AnEventAppended(EventViewModel eventViewModel);
}