namespace Quantum.EventSourcing.Subscriber;

public interface ICatchUpSubscriber : ISubscriber
{
    void LiveProcessingStarted();
}