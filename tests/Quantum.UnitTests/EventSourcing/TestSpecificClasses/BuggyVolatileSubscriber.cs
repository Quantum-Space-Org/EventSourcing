using Quantum.EventSourcing;
using Quantum.EventSourcing.Subscriber;

namespace Quantum.UnitTests.EventSourcing.TestSpecificClasses
{
    public class BuggyVolatileSubscriber : ISubscriber, ICatchUpSubscriber
    {
        private Type ExpectedEventType { get; set; }
        public Type ActualEventTypeReceived { get; set; }

        public int ActualTimesOfCalled;

        public static BuggyVolatileSubscriber WhichIExpectTheLastEventShouldBe(Type type)
            => new BuggyVolatileSubscriber
            {
                ExpectedEventType = type
            };


        public string Name => nameof(BuggyVolatileSubscriber);

        public void AnEventAppended(EventViewModel eventViewModel)
        {
            ActualTimesOfCalled++;
            ActualEventTypeReceived = Type.GetType(eventViewModel.EventType);
            throw new NotImplementedException();
        }


        public void LiveProcessingStarted()
        {
            throw new NotImplementedException();
        }

        public void Verify()
        {
            ActualEventTypeReceived.Should().Be(ExpectedEventType);
        }
    }
}