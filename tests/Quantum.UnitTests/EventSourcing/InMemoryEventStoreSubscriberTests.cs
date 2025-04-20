using Quantum.Core;
using Quantum.EventSourcing;
using Quantum.EventSourcing.InMemoryEventStore;
using Quantum.EventSourcing.Subscriber;
using Quantum.UnitTests.EventSourcing.CustomerAggregate;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;
using Quantum.UnitTests.EventSourcing.TestSpecificClasses;

namespace Quantum.UnitTests.EventSourcing
{
    public class InMemoryEventStoreSubscriberTests
    {
        private readonly IEventStore _inMemoryEventStore;

        public InMemoryEventStoreSubscriberTests()
        {
            IDateTimeProvider dateTimeProvider = new DummyDateTimeProvider();

            _inMemoryEventStore = new InMemoryEventStore(dateTimeProvider);
        }

        [Fact]
        public async Task Given_ThereAreManySubscribersSubscribeToAnEventStream_When_AnEventAppended_Then_AllSubscribersWillBeNotified()
        {
            var customerId = new CustomerId("1");

            var customerNameIsChangedEvent = new CustomerNameIsChangedEvent(customerId.Id, "customer1 name", "customer1 family");
            var appendEventDto = AppendEventDto.Version1(customerNameIsChangedEvent);

            //subscriber1
            ISubscriber subscriber1 = MockVolatileSubscriber.WhichItIsExpectedToBeCalledWithEvent("subscriber 1", EventViewModelFactory.CreateEventViewModel(customerNameIsChangedEvent, appendEventDto));
            await _inMemoryEventStore.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber1);

            //subscriber2
            ISubscriber subscriber2 = MockVolatileSubscriber.WhichItIsExpectedToBeCalledWithEvent("subscriber 2", EventViewModelFactory.CreateEventViewModel(customerNameIsChangedEvent, appendEventDto));
            await _inMemoryEventStore.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber2);

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, appendEventDto);

            ((MockVolatileSubscriber)subscriber1).Verify();
            ((MockVolatileSubscriber)subscriber2).Verify();
        }

        [Fact]
        public async Task Given_EventStreamDoesNotExist_When_SubscribingToThem_Then_EventStreamWillBeCreatedAndSubscribedToThem()
        {
            var customerId = new CustomerId("1");

            var customerNameIsChangedEvent = new CustomerNameIsChangedEvent(customerId.Id, "customer1 name", "customer1 family");
            var appendEventDto = AppendEventDto.Version1(customerNameIsChangedEvent);

            ISubscriber subscriber = MockVolatileSubscriber.WhichItIsExpectedToBeCalledWithEvent("subscriber", EventViewModelFactory.CreateEventViewModel(customerNameIsChangedEvent, appendEventDto));

            await _inMemoryEventStore.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber);

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, appendEventDto);

            ((MockVolatileSubscriber)subscriber).Verify();
        }

        [Fact]
        public async Task Given_ThereAreManySubscribersSubscribedToAllEventStream_When_AnEventAppended_Then_AllSubscribersWillBeNotified()
        {
            //subscriber to all events stream
            var subscriber1 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 1", 2);

            var subscriber2 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 2", 2);

            await _inMemoryEventStore.SubscribeToAllEventStream(subscriber: subscriber1);
            await _inMemoryEventStore.SubscribeToAllEventStream(subscriber: subscriber2);

            //append to the first stream
            var customerId1 = new CustomerId("1");
            var appendEventDto1 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, "customer1 name", "customer1 family"));
            await _inMemoryEventStore.AppendToEventStreamAsync(customerId1, appendEventDto1);

            //append to the second stream
            var customerId2 = new CustomerId("2");
            var appendEventDto2 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId2.Id, "customer2 name", "customer2 family"));
            await _inMemoryEventStore.AppendToEventStreamAsync(customerId2, appendEventDto2);

            //subscriber should informed about events appended to all event stream
            subscriber1.VerifyThatIsIsCalledAsExpected();
            subscriber2.VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async Task VolatileSubscribeToAllEventStream()
        {
            //subscriber to all events stream
            ISubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 2);
            await _inMemoryEventStore.SubscribeToAllEventStream(subscriber: subscriber);

            //append to the first stream
            var customerId1 = new CustomerId("1");
            var appendEventDto1 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, "customer1 name", "customer1 family"));
            await _inMemoryEventStore.AppendToEventStreamAsync(customerId1, appendEventDto1);

            //append to the second stream
            var customerId2 = new CustomerId("2");
            var appendEventDto2 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId2.Id, "customer2 name", "customer2 family"));
            await _inMemoryEventStore.AppendToEventStreamAsync(customerId2, appendEventDto2);

            //subscriber should informed about events appended to all event stream
            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async void CatchUpSubscription()
        {
            var customerId = new CustomerId("1");
            var events = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                var @event = new CustomerNameIsChangedEvent(customerId.Id, $"customer {i} name", $"customer {i} family");
                events.Add(@event);
            }

            var toBeAppendedEvents = events
                .Select(AppendEventDto.Version1)
                .ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, toBeAppendedEvents);

            ICatchUpSubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 6);
            await _inMemoryEventStore.CatchUpSubscribeToAllEventStream(subscriber,
                startFrom: EventStreamPosition.AtStart());

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId.Id, "Greg", "Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));

            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async void CatchUpSubscription_ManySubscribers()
        {
            var customerId = new CustomerId("1");
            var events = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                var @event = new CustomerNameIsChangedEvent(customerId.Id, $"customer {i} name", $"customer {i} family");
                events.Add(@event);
            }

            var toBeAppendedEvents = events
                .Select(AppendEventDto.Version1)
                .ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, toBeAppendedEvents);

            ICatchUpSubscriber subscriber1 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 1", 6);
            ICatchUpSubscriber subscriber2 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 2", 6);

            await _inMemoryEventStore.CatchUpSubscribeToAllEventStream(subscriber1,
                startFrom: EventStreamPosition.AtStart());
            await _inMemoryEventStore.CatchUpSubscribeToAllEventStream(subscriber2,
                startFrom: EventStreamPosition.AtStart());

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId.Id, "Greg", "Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber1).LiveProcessingIsStarted
                              && ((MockVolatileSubscriber)subscriber2).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));

            ((MockVolatileSubscriber)subscriber1).VerifyThatIsIsCalledAsExpected();
            ((MockVolatileSubscriber)subscriber2).VerifyThatIsIsCalledAsExpected();
        }


        [Fact]
        public async void CatchUpSubscriptionToAllEventStream()
        {
            var customerId1 = new CustomerId("1");

            var events1 = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                events1.Add(new CustomerNameIsChangedEvent(customerId1.Id, $"customer {i} name", $"customer {i} family"));
            }

            var toBeAppendedEvents1 = events1
                .Select(AppendEventDto.Version1)
                .ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId1, toBeAppendedEvents1);

            ICatchUpSubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 6);

            await _inMemoryEventStore.CatchUpSubscribeToAllEventStream(subscriber, startFrom: EventStreamPosition.AtStart());

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId1, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, "Greg", "Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));


            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async void CatchUpSubscription_SubscribeToSpecificEventStream()
        {
            var customerId1 = new CustomerId("1");

            var events1 = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                events1.Add(new CustomerNameIsChangedEvent(customerId1.Id, $"customer {i} name", $"customer {i} family"));
            }

            var toBeAppendedEvents1 = events1
                .Select(AppendEventDto.Version1)
                .ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId1, toBeAppendedEvents1);

            ICatchUpSubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 6);

            await _inMemoryEventStore.CatchUpSubscribeToEventStream(customerId1, subscriber, startFrom: EventStreamPositions.FromStart);

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId1, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, "Greg", "Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));

            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async void Give_SubscriberHasAnError_When_ItIsNotified_Then_Subscribers_Error_Should_Not_Affect_The_Event_Store()
        {
            var customerId = new CustomerId("1");

            var customerNameIsChangedEvent = new CustomerNameIsChangedEvent(customerId.Id, "customer1 name", "customer1 family");
            var appendEventDto = AppendEventDto.Version1(customerNameIsChangedEvent);

            ISubscriber subscriber = BuggyVolatileSubscriber.WhichIExpectTheLastEventShouldBe(typeof(CustomerNameIsChangedEvent));

            await _inMemoryEventStore.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber);

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, appendEventDto);

            await Waiter.Wait(() => ((BuggyVolatileSubscriber)subscriber).ActualTimesOfCalled == 0, TimeSpan.FromSeconds(3));

            ((BuggyVolatileSubscriber)subscriber).Verify();
        }
    }
}