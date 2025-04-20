using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Quantum.EventSourcing;
using Quantum.EventSourcing.EventStoreDB;
using Quantum.EventSourcing.Subscriber;
using Quantum.IntegrationTests.EventSourcing.CustomerAggregate;
using Quantum.IntegrationTests.EventSourcing.CustomerAggregate.Events;
using Quantum.IntegrationTests.EventSourcing.TestSpecificClasses;
using Xunit;

namespace Quantum.IntegrationTests.EventSourcing
{
    public class EventStoreDbSubscriberTests : IClassFixture<EventStoreDbEventStoreTestsBase>
    {
        private readonly IEventStore _eventStoreDb;

        public EventStoreDbSubscriberTests(EventStoreDbEventStoreTestsBase eventStoreDbEventStoreTestsBase)
            => _eventStoreDb = new EventStoreDbEventStore(eventStoreDbEventStoreTestsBase.EventStoreDbConfig, new NullLogger<EventStoreDbEventStore>());

        #region
        [Fact]
        public async Task VolatileSubscribeToAllEventStream()
        {
            //subscriber to all events stream
            ISubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 2);
            await _eventStoreDb.SubscribeToAllEventStream(subscriber: subscriber);

            //append to the first stream
            var customerId1 = new CustomerId("1");
            var appendEventDto1 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, $"customer1 name", $"customer1 family"));
            await _eventStoreDb.AppendToEventStreamAsync(customerId1, appendEventDto1);

            //append to the second stream
            var customerId2 = new CustomerId("2");
            var appendEventDto2 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId2.Id, $"customer2 name", $"customer2 family"));
            await _eventStoreDb.AppendToEventStreamAsync(customerId2, appendEventDto2);

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).CallTimes(2), TimeSpan.FromSeconds(5));

            //subscriber should informed about events appended to all event stream
            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }
        
        [Fact]
        public async Task CatchUpSubscription()
        {
            var customerId = new CustomerId("1");
            var events = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                var @event = new CustomerNameIsChangedEvent(customerId.Id, $"customer {i} name", $"customer {i} family");
                @events.Add(@event);
            }

            var toBeAppendedEvents = events
                .Select(AppendEventDto.Version1)
                .ToList();

            await _eventStoreDb.AppendToEventStreamAsync(customerId, toBeAppendedEvents);

            ICatchUpSubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 6);
            await _eventStoreDb.CatchUpSubscribeToAllEventStream(subscriber, startFrom: EventStreamPosition.AtStart());

            await _eventStoreDb.AppendToEventStreamAsync(customerId, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId.Id, $"Greg", $"Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));

            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async Task CatchUpSubscription_ManySubscribers()
        {
            var customerId = new CustomerId("1");
            var events = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                var @event = new CustomerNameIsChangedEvent(customerId.Id, $"customer {i} name", $"customer {i} family");
                @events.Add(@event);
            }

            var toBeAppendedEvents = events
                .Select(AppendEventDto.Version1)
                .ToList();

            await _eventStoreDb.AppendToEventStreamAsync(customerId, toBeAppendedEvents);

            ICatchUpSubscriber subscriber1 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 1", 6);
            ICatchUpSubscriber subscriber2 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 2", 6);

            await _eventStoreDb.CatchUpSubscribeToAllEventStream(subscriber1,
                startFrom: EventStreamPosition.AtStart());
            await _eventStoreDb.CatchUpSubscribeToAllEventStream(subscriber2,
                startFrom: EventStreamPosition.AtStart());

            await _eventStoreDb.AppendToEventStreamAsync(customerId, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId.Id, $"Greg", $"Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber1).LiveProcessingIsStarted
                              && ((MockVolatileSubscriber)subscriber2).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));

            ((MockVolatileSubscriber)subscriber1).VerifyThatIsIsCalledAsExpected();
            ((MockVolatileSubscriber)subscriber2).VerifyThatIsIsCalledAsExpected();
        }


        [Fact]
        public async Task ReconnectToEventStoreDB_CatchUpSubscription_SubscribersIsTooSlow()
        {
            var customerId = new CustomerId("1");
            var events = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                var @event = new CustomerNameIsChangedEvent(customerId.Id, $"customer {i} name", $"customer {i} family");
                @events.Add(@event);
            }

            var toBeAppendedEvents = events
                .Select(AppendEventDto.Version1)
                .ToList();

            await _eventStoreDb.AppendToEventStreamAsync(customerId, toBeAppendedEvents);

            ICatchUpSubscriber subscriber1 = new VerySlowCatchupSubscriber();
            
            await _eventStoreDb.CatchUpSubscribeToAllEventStream(subscriber1, startFrom: EventStreamPosition.AtStart());
            
            await _eventStoreDb.AppendToEventStreamAsync(customerId, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId.Id, $"Greg", $"Young")));

            await Waiter.Wait(() => ((VerySlowCatchupSubscriber)subscriber1).LiveProcessingIsStarted ,TimeSpan.FromSeconds(10));

            ((VerySlowCatchupSubscriber)subscriber1).LiveProcessingIsStarted.Should().BeFalse();
        }

        [Fact]
        public async Task CatchUpSubscriptionToAllEventStream()
        {
            var customerId1 = new CustomerId("1");

            var domainEvents = new List<IsADomainEvent>();

            //created 5 domain events
            for (var i = 0; i < 5; i++)
            {
                domainEvents.Add(new CustomerNameIsChangedEvent(customerId1.Id, $"customer {i} name", $"customer {i} family"));
            }

            var toBeAppendedEvents = domainEvents
                .Select(AppendEventDto.Version1)
                .ToList();

            //event events to event stream
            await _eventStoreDb.AppendToEventStreamAsync(customerId1, toBeAppendedEvents);

            //catchup subscribe to the $all event stream
            ICatchUpSubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 6);
            await _eventStoreDb.CatchUpSubscribeToAllEventStream(subscriber, startFrom: EventStreamPosition.AtStart());

            await _eventStoreDb.AppendToEventStreamAsync(customerId1, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, $"Greg", $"Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));


            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async Task Give_SubscriberHasAnError_When_ItIsNotified_Then_Subscriber_s_Error_Should_Not_Affect_The_Event_Store()
        {
            //subscribe to the $all event stream
            ISubscriber subscriber = BuggyVolatileSubscriber.WhichIExpectTheLastEventShouldBe(typeof(CustomerNameIsChangedEvent) );
            await _eventStoreDb.SubscribeToAllEventStream(subscriber: subscriber);

            var customerId = new CustomerId("1");

            //Append event 1 to event stream
            var customerIsCreatedEvent = new ANewCustomerIsCreatedEvent(customerId.Id, $"customer1 name", $"customer1 family");
            var appendEventDto = AppendEventDto.Version1(customerIsCreatedEvent);
            await _eventStoreDb.AppendToEventStreamAsync(customerId, appendEventDto);

            //await Waiter.Wait(() => ((BuggyVolatileSubscriber)subscriber).ActualTimesOfCalled == 0, TimeSpan.FromSeconds(3));

            //append event two to event store
            var customerNameIsChangedEvent= new CustomerNameIsChangedEvent(customerId.Id, $"customer1 name", $"customer1 family");
            appendEventDto = AppendEventDto.Version1(customerNameIsChangedEvent);
            await _eventStoreDb.AppendToEventStreamAsync(customerId, appendEventDto);
            await Waiter.Wait(() => ((BuggyVolatileSubscriber)subscriber).ActualEventTypeReceived == typeof(CustomerNameIsChangedEvent), TimeSpan.FromSeconds(4));

            ((BuggyVolatileSubscriber)subscriber).Verify();
        }
        
        [Fact]
        public async Task Give_ThereAre5EventsInEventStream_When_SubscribeFromEvent4Position_Then_Ill_Get_Just_Two_Last_Event()
        {
            var customerId1 = new CustomerId("1");

            var events1 = new List<IsADomainEvent>();

            for (var i = 0; i < 4; i++)
            {
                events1.Add(new CustomerNameIsChangedEvent(customerId1.Id, $"customer {i} name", $"customer {i} family"));
            }

            var toBeAppendedEvents1 = events1
                .Select(AppendEventDto.Version1)
                .ToList();

            await _eventStoreDb.AppendToEventStreamAsync(customerId1, toBeAppendedEvents1);

            var events =  await _eventStoreDb.LoadEventsFromAllEventStreamAsync(EventStreamPosition.AtStart());

            var theFourthEvent = events.Events.ToArray()[2];


            ICatchUpSubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 2);

            await _eventStoreDb.CatchUpSubscribeToAllEventStream(subscriber, startFrom: new EventStreamPosition(theFourthEvent.GlobalCommitPosition , theFourthEvent.GlobalPreparePosition));

            await _eventStoreDb.AppendToEventStreamAsync(customerId1, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, $"Greg", $"Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));

            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();

        }

        [Fact]
        public async Task Given_ThereAreManySubscribersSubscribedToAllEventStream_When_AnEventAppended_Then_AllSubscribersWillBeNotified()
        {
            //subscriber to all events stream
            var subscriber1 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 1", 2);

            var subscriber2 = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber 2", 2);

            await _eventStoreDb.SubscribeToAllEventStream(subscriber: subscriber1);
            await _eventStoreDb.SubscribeToAllEventStream(subscriber: subscriber2);

            //append to the first stream
            var customerId1 = new CustomerId("1");
            var appendEventDto1 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, $"customer1 name", $"customer1 family"));
            await _eventStoreDb.AppendToEventStreamAsync(customerId1, appendEventDto1);

            //append to the second stream
            var customerId2 = new CustomerId("2");
            var appendEventDto2 = AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId2.Id, $"customer2 name", $"customer2 family"));
            await _eventStoreDb.AppendToEventStreamAsync(customerId2, appendEventDto2);

            await Waiter.Wait(() => subscriber1.CallTimes(2) && subscriber1.CallTimes(2), TimeSpan.FromSeconds(5));

            //subscriber should informed about events appended to all event stream
            subscriber1.VerifyThatIsIsCalledAsExpected();
            subscriber2.VerifyThatIsIsCalledAsExpected();
        }

        #endregion

        [Fact]
        public async Task Given_EventStreamDoesNotExist_When_SubscribingToThem_Then_EventStreamWillBeCreatedAndSubscribedToThem()
        {
            var customerId = new CustomerId("1");

            var customerNameIsChangedEvent = new CustomerNameIsChangedEvent(customerId.Id, $"customer1 name", $"customer1 family");
            var appendEventDto = AppendEventDto.Version1(customerNameIsChangedEvent);

            ISubscriber subscriber = MockVolatileSubscriber.WhichItIsExpectedToBeCalledWithEvent("subscriber", CreateEventViewModel(customerNameIsChangedEvent, appendEventDto));

           await _eventStoreDb.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber);

            await _eventStoreDb.AppendToEventStreamAsync(customerId, appendEventDto);

            ((MockVolatileSubscriber)subscriber).Verify();
        }
        
        [Fact]
        public async Task CatchUpSubscription_SubscribeToSpecificEventStream()
        {
            var customerId1 = new CustomerId("1");

            var events1 = new List<IsADomainEvent>();

            for (var i = 0; i < 5; i++)
            {
                events1.Add(new CustomerNameIsChangedEvent(customerId1.Id, $"customer {i} name", $"customer {i} family"));
            }

            var toBeAppendedEvents = events1
                .Select(AppendEventDto.Version1)
                .ToList();

            await _eventStoreDb.AppendToEventStreamAsync(customerId1, toBeAppendedEvents);

            ICatchUpSubscriber subscriber = MockVolatileSubscriber.WhichIsExpectedToBeCalledTimes("subscriber", 6);

            await _eventStoreDb.CatchUpSubscribeToEventStream(customerId1, subscriber, startFrom: EventStreamPositions.FromStart);

            await _eventStoreDb.AppendToEventStreamAsync(customerId1, AppendEventDto.Version1(new CustomerNameIsChangedEvent(customerId1.Id, $"Greg", $"Young")));

            await Waiter.Wait(() => ((MockVolatileSubscriber)subscriber).LiveProcessingIsStarted, TimeSpan.FromSeconds(3));

            ((MockVolatileSubscriber)subscriber).VerifyThatIsIsCalledAsExpected();
        }

        [Fact]
        public async Task Given_ThereAreManySubscribersSubscribeToAnEventStream_When_AnEventAppended_Then_AllSubscribersWillBeNotified()
        {
            var customerId = new CustomerId("1");

            var customerNameIsChangedEvent = new CustomerNameIsChangedEvent(customerId.Id, $"customer1 name", $"customer1 family");
            var appendEventDto = AppendEventDto.Version1(customerNameIsChangedEvent);

            //subscriber1
            ISubscriber subscriber1 = MockVolatileSubscriber.WhichItIsExpectedToBeCalledWithEvent("subscriber 1", CreateEventViewModel(customerNameIsChangedEvent, appendEventDto));
            await _eventStoreDb.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber1);

            //subscriber2
            ISubscriber subscriber2 = MockVolatileSubscriber.WhichItIsExpectedToBeCalledWithEvent("subscriber 2", CreateEventViewModel(customerNameIsChangedEvent, appendEventDto));
            await _eventStoreDb.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber2);

            await _eventStoreDb.AppendToEventStreamAsync(customerId, appendEventDto);

            ((MockVolatileSubscriber)subscriber1).Verify();
            ((MockVolatileSubscriber)subscriber2).Verify();
        }

        private static EventViewModel CreateEventViewModel(CustomerNameIsChangedEvent customerNameIsChangedEvent, AppendEventDto appendEventDto)
        {
            return new()
            {
                Version = 1,
                Payload = customerNameIsChangedEvent,
                EventType = typeof(CustomerNameIsChangedEvent).AssemblyQualifiedName,
                GlobalCommitPosition = 0,
                Metadata = appendEventDto.Metadata,
                EventId = appendEventDto.GlobalUniqueEventId
            };
        }
    }

    public class VerySlowCatchupSubscriber : ICatchUpSubscriber
    {
        public string Name { get; } = "VerySlowCatchupSubscriber";
        public bool LiveProcessingIsStarted { get; set; } = false;

        public void AnEventAppended(EventViewModel eventViewModel)
        {
            Task.Delay(4).Wait();
        }

        public void LiveProcessingStarted()
        {
            LiveProcessingIsStarted = true;
        }
    }
}