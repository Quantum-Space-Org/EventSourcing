using System;
using FluentAssertions;
using Quantum.EventSourcing;
using Quantum.EventSourcing.Subscriber;

namespace Quantum.IntegrationTests.EventSourcing.TestSpecificClasses
{
    public class BuggyVolatileSubscriber : ISubscriber, ICatchUpSubscriber
    {
        private Type ExpectedEventType { get; set; }
        public Type ActualEventTypeReceived { get; set; }

        public int ActualTimesOfCalled;

        public static BuggyVolatileSubscriber WhichIExpectTheLastEventShouldBe(Type type)
            => new BuggyVolatileSubscriber()
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
    public class MockVolatileSubscriber : ISubscriber, ICatchUpSubscriber
    {
        public string Name { get; }
        public bool LiveProcessingIsStarted { get; set; }

        private EventViewModel _expectedEvent;
        private EventViewModel _actualEvent;
        private int _theNumberOfTimesItIsExpectedToBeCalled;
        private int _actualNumberOfTimesItIsCalled;
        public static MockVolatileSubscriber WhichIsExpectedToBeCalledTimes(string name, int theNumberOfTimesItIsExpectedToBeCalled)
            => new(name: name)
            {
                _theNumberOfTimesItIsExpectedToBeCalled = theNumberOfTimesItIsExpectedToBeCalled
            };

        public static MockVolatileSubscriber WhichItIsExpectedToBeCalledWithEvent(string name, EventViewModel toEventViewModel)
            => new(name) { _expectedEvent = toEventViewModel };

        public MockVolatileSubscriber(string name)
            => Name = name;


        public void AnEventAppended(EventViewModel eventViewModel)
        {
            _actualNumberOfTimesItIsCalled++;
            _actualEvent = eventViewModel;
        }

        public void LiveProcessingStarted()
            => LiveProcessingIsStarted = true;

        public void Verify()
        {
            if (_expectedEvent == null)
                throw new Exception($"MockVolatileSubscriber --> Actual Event is not configured. You should set your expected event to called.");

            _expectedEvent.Should().NotBeNull();

            _expectedEvent.Version.Should().Be(_actualEvent.Version);
            _expectedEvent.EventId.Should().Be(_actualEvent.EventId);
            _expectedEvent.EventType.Should().Be(_actualEvent.EventType);
            _expectedEvent.Metadata.Should().Be(_actualEvent.Metadata);
            _expectedEvent.GlobalCommitPosition.Should().Be(_actualEvent.GlobalCommitPosition);
            _expectedEvent.Payload.Should().Be(_actualEvent.Payload);
        }
        public void VerifyThatIsIsCalledAsExpected()
            => _actualNumberOfTimesItIsCalled.Should().Be(_theNumberOfTimesItIsExpectedToBeCalled);

        public bool CallTimes(int times) => _actualNumberOfTimesItIsCalled == times;
    }
}