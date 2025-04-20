using Quantum.EventSourcing.Subscriber;
using System;

namespace Quantum.EventSourcing.InMemoryEventStore
{
    [Serializable]
    public class SubscriberNameDuplicationException : Exception
    {
        public SubscriberNameDuplicationException(ISubscriber subscriber)
        {
            Subscriber = subscriber;
        }


        public ISubscriber Subscriber { get; }
    }
}