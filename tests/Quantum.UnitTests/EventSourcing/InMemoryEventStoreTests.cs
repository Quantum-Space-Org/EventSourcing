using Quantum.Core;
using Quantum.EventSourcing;
using Quantum.EventSourcing.Exceptions;
using Quantum.EventSourcing.InMemoryEventStore;
using Quantum.UnitTests.Domain.HelperClasses;
using Quantum.UnitTests.EventSourcing.CustomerAggregate;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;
using Quantum.UnitTests.EventSourcing.TestSpecificClasses;

namespace Quantum.UnitTests.EventSourcing
{
    public class InMemoryEventStoreTests
    {
        private readonly IEventStore _inMemoryEventStore;
        private readonly IDateTimeProvider _dateTimeProvider;


        public InMemoryEventStoreTests()
        {
            _dateTimeProvider = new DummyDateTimeProvider();

            _inMemoryEventStore = new InMemoryEventStore(_dateTimeProvider);
        }

        [Fact]
        public async Task Load_SpecifiedEventStreamDoesNotExists_AnEventStreamWith0VersionAndWithoutAnyEventsWillBeReturned()
        {
            var eventStream = await _inMemoryEventStore.LoadEventStreamAsync(new FakeEntityIdentity(1));

            eventStream.Version.Should().Be(0);
            eventStream.Events.Should().BeEmpty();
        }

        [Fact]
        public async Task Given_LocalPositionOfLastEventIs0_WhenANewEventIsAppended_LocalPositionOfLastEventWillBe1()
        {
            //Given
            var customer = CreateCustomer("1");
            var firstEventViewModel = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, firstEventViewModel);

            //When
            customer.ChangeName(new FullName("Greg", "Young"));
            var secondEventViewModel = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, secondEventViewModel);

            //ThenIWillExpect
            var eventStream = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, EventStreamPosition.AtStart());
            eventStream.Events.Last().GlobalCommitPosition.Should().Be(1);
        }

        [Fact]
        public async Task Given_2VersionWereExist_When_LoadVersion2OfEventStream_Then_TheEventsOfTheSecondVersionWillOnlyBeReturned()
        {
            //Given
            var customer = CreateCustomer("1");
            var firstEventViewModel = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());
            //version 1
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, firstEventViewModel);

            customer.ChangeName(new FullName("Greg", "Young"));
            customer.ChangeName(new FullName("Greg", "Young"));

            var toBeAppendEvents = new List<AppendEventDto>();
            foreach (var @event in customer.DeQueueDomainEvents())
            {
                var secondEventViewModel = AppendEventDto.Version1(payload: @event);
                toBeAppendEvents.Add(secondEventViewModel);
            }

            //version 2
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, toBeAppendEvents);

            //When
            var eventStream = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, EventStreamPosition.AtStart(), version: (EventStreamVersion)2);

            //ThenIWillExpect
            eventStream.Version.Should().Be(2);
            eventStream.Count.Should().Be(2);
        }

        [Fact]
        public async Task LoadEvent_EventStreamDoesNotExists_ExceptionWillBeThrown()
        {
            var fakeEntityIdentity = new FakeEntityIdentity(1);

            Action action = () => _inMemoryEventStore.LoadEventAsync(fakeEntityIdentity, EventStreamPosition.AtEnd()).Wait();
            action.Should().Throw<EventStreamNotExistsException>();
        }

        [Fact]
        public async Task LoadEvent_EventPositionIsWrong_ExceptionWillBeThrown()
        {
            var customer = CreateCustomer("1");
            var toBeAppendEvents = customer.DeQueueDomainEvents().Select(AppendEventDto.Version1).ToList();
            //event will be appended at commitPosition 1.
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, toBeAppendEvents);

            //if we fetch events from any commitPosition other than 1, exception should be thrown.
            Action action = () => _inMemoryEventStore.LoadEventAsync(customer.Identity, EventStreamPosition.AtEnd()).Wait();
            action.Should().Throw<EventPositionIsWrongException>();
        }

        [Fact]
        public async Task Given_2EventsAreAppendedToEventStreamAtPosition0And1_When_LoadingEventAtEachPosition_Then_TheAppropriateEventWillBeLoaded()
        {
            //Appending the first event at commitPosition 1
            var customer = CreateCustomer("1");
            var aCustomerIsCreatedEvent = customer.DeQueueDomainEvents().Single();
            var firstEventViewModel = AppendEventDto.Version1(payload: aCustomerIsCreatedEvent);
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, firstEventViewModel);

            //Appending the second event at commitPosition 2
            customer.ChangeName(new FullName("Greg", "Young"));
            var customerNameIsChangedEvent = customer.DeQueueDomainEvents().Single();
            var secondEventViewModel = AppendEventDto.Version1(payload: customerNameIsChangedEvent);
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, secondEventViewModel);

            //load event by commitPosition
            var theEventAtPosition1 = await _inMemoryEventStore.LoadEventAsync(customer.Identity, new EventStreamPosition(0 , 0));
            var theEventAtPosition2 = await _inMemoryEventStore.LoadEventAsync(customer.Identity, new EventStreamPosition(1, 1));

            theEventAtPosition1.Payload.Should().BeEquivalentTo(aCustomerIsCreatedEvent);
            theEventAtPosition2.Payload.Should().BeEquivalentTo(customerNameIsChangedEvent);
        }


        [Fact]
        public async Task LoadEventByEventId()
        {
            //Appending the first event at commitPosition 1
            var customer = CreateCustomer("1");
            var aCustomerIsCreatedEvent = customer.DeQueueDomainEvents().Single();
            var firstEventViewModel = AppendEventDto.Version1(payload: aCustomerIsCreatedEvent);
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, firstEventViewModel);

            EventViewModel @event = await _inMemoryEventStore.LoadEventAsync(customer.Identity, firstEventViewModel.GlobalUniqueEventId);
            @event.Payload.Should().BeEquivalentTo(aCustomerIsCreatedEvent);
        }

        [Fact]
        public async Task LoadEventsFromAllEventStream()
        {
            //Append the first event to event stream 1
            var customer1 = CreateCustomer("1");
            var eventViewModel = AppendEventDto.Version1(payload: customer1.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel);

            //Append the second event to event stream 2
            var customer2 = CreateCustomer("2");
            eventViewModel = AppendEventDto.Version1(payload: customer2.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer2.Identity, eventViewModel);

            //Append the thirth event to event stream 1
            customer1.ChangeName(new FullName("Eric", "Evans"));
            eventViewModel = AppendEventDto.Version1(payload: customer1.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel);

            //Append the fourth event to event stream 1
            customer2.ChangeName(new FullName("Greg", "Young"));
            eventViewModel = AppendEventDto.Version1(payload: customer2.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer2.Identity, eventViewModel);

            var allEventStream = await _inMemoryEventStore.LoadEventsFromAllEventStreamAsync(EventStreamPosition.AtStart());

            allEventStream.Count.Should().Be(4);
        }

        [Fact]
        public async Task LoadEventsFromAllEventStream_SkipAndTake()
        {
            //Append the first event to event stream 1
            var customer1 = CreateCustomer("1");
            var eventViewModel = AppendEventDto.Version1(payload: customer1.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel);

            //Append the second event to event stream 2
            var customer2 = CreateCustomer("2");
            eventViewModel = AppendEventDto.Version1(payload: customer2.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer2.Identity, eventViewModel);

            //Append the thirth event to event stream 1
            customer1.ChangeName(new FullName("Eric", "Evans"));
            var thirdDomainEvent = customer1.DeQueueDomainEvents().Single();
            eventViewModel = AppendEventDto.Version1(payload: thirdDomainEvent);
            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel);

            //Append the fourth event to event stream 1
            customer2.ChangeName(new FullName("Greg", "Young"));
            var fourthDomainEvent = customer2.DeQueueDomainEvents().Single();
            eventViewModel = AppendEventDto.Version1(payload: fourthDomainEvent);
            await _inMemoryEventStore.AppendToEventStreamAsync(customer2.Identity, eventViewModel);

            //When
            var allEventStream = await _inMemoryEventStore.LoadEventsFromAllEventStreamAsync(new EventStreamPosition(2 , 2), maxCount: 2);

            //ThenIWillExpect
            allEventStream.Count.Should().Be(2);
            allEventStream.Events.First().Payload.Should().BeEquivalentTo(thirdDomainEvent);
            allEventStream.Events.Last().Payload.Should().BeEquivalentTo(fourthDomainEvent);
        }

        [Fact]
        public async Task TestNextPage()
        {
            var customer1 = CreateCustomer("1");
            customer1.ChangeName(new FullName("Eric", "Evans"));
            customer1.ChangeName(new FullName("Greg", "Young"));
            customer1.ChangeName(new FullName("Martin", "Fowler"));
            customer1.ChangeName(new FullName("Uncle", "Bob"));

            var toBeAppendedEvents = customer1.DeQueueDomainEvents()
                .Select(AppendEventDto.Version1)
                .ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, toBeAppendedEvents);

            //When
            var allEventStream = await _inMemoryEventStore.LoadEventsFromAllEventStreamAsync(EventStreamPosition.AtStart(), maxCount: 3);

            allEventStream.HasAnyEventYet.Should().BeTrue();
            allEventStream.NextPage.NextSkip.Should().Be(4);
            allEventStream.NextPage.NextTake.Should().Be(2);
            allEventStream.RemainingEventCount.Should().Be(2);
        }


        [Fact]
        public async Task TestNextPage_Backward_FromAllEventStream()
        {
            var customerId = new CustomerId("1");
            var events = new List<IsADomainEvent>();

            for (var i = 0; i < 1000; i++)
            {
                var @event = new CustomerNameIsChangedEvent(customerId.Id, $"customer {i} name", $"customer {i} family");
                events.Add(@event);
            }

            var toBeAppendedEvents = events
                .Select(AppendEventDto.Version1)
                .ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, toBeAppendedEvents);

            //When
            var allEventStream = await _inMemoryEventStore.LoadEventsFromAllEventStreamBackwardAsync(EventStreamPositions.FromEnd, maxCount: 10);

            allEventStream.HasAnyEventYet.Should().BeTrue();
            allEventStream.RemainingEventCount.Should().Be(1000 - 10);
            allEventStream.NextPage.NextSkip.Should().Be(990);
            allEventStream.NextPage.NextTake.Should().Be(10);
        }


        [Fact]
        public async Task TestNextPage_Backward_FromSpecificEventStream()
        {
            var customerId = new CustomerId("1");
            var events = new List<IsADomainEvent>();

            for (var i = 0; i < 1000; i++)
            {
                var @event = new CustomerNameIsChangedEvent(customerId.Id, $"customer {i} name", $"customer {i} family");
                events.Add(@event);
            }

            var toBeAppendedEvents = events
                .Select(AppendEventDto.Version1)
                .ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(customerId, toBeAppendedEvents);

            //When
            var allEventStream = await _inMemoryEventStore.LoadEventStreamBackwardAsync(customerId, EventStreamVersion.Any, EventStreamPositions.FromEnd, maxCount: 10);

            allEventStream.HasAnyEventYet.Should().BeTrue();
            allEventStream.RemainingEventCount.Should().Be(1000 - 10);
            allEventStream.NextPage.NextSkip.Should().Be(990);
            allEventStream.NextPage.NextTake.Should().Be(10);
        }


        [Fact]
        public async Task Given_From2DaysAgoUntilTodayEventsAreAppended_When_YouFetchEventsOfYesterdayAndToday_Then_OnlyEventsThatOccurredDuringTheSpecifiedPeriodWillBeLoaded()
        {
            //Given
            //the day before yesterday
            var theDayBeforeYesterday = DateTimeOffset.UtcNow.AddDays(-2);
            var yesterday = DateTimeOffset.UtcNow.AddDays(-1);
            var today = DateTimeOffset.UtcNow.AddDays(-1);

            //config date time provider for the first 3 events to two days ago
            ((DummyDateTimeProvider)_dateTimeProvider).WhenCallUtcNowReturn(theDayBeforeYesterday);

            var customer = CreateCustomer("1");
            customer.ChangeName(new FullName("Greg", "Young"));
            customer.ChangeName(new FullName("Martin", "Fowler"));
            var toBeAppendEvents = customer.DeQueueDomainEvents().Select(AppendEventDto.Version1).ToList();
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, toBeAppendEvents);

            //config date time provider to yesterday because, I events which i want to appended at the time of yesterday
            ((DummyDateTimeProvider)_dateTimeProvider).WhenCallUtcNowReturn(yesterday);

            customer.ChangeName(new FullName("Eric", "Evans"));
            customer.ChangeName(new FullName("Eric", "Evans"));
            toBeAppendEvents = customer.DeQueueDomainEvents().Select(AppendEventDto.Version1).ToList();
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, toBeAppendEvents);

            //config date time provider to today
            ((DummyDateTimeProvider)_dateTimeProvider).WhenCallUtcNowReturn(today);
            customer.ChangeName(new FullName("Kent", "Beck"));

            toBeAppendEvents = customer.DeQueueDomainEvents().Select(AppendEventDto.Version1).ToList();

            //Append 5 events
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, toBeAppendEvents);
            customer.ChangeName(new FullName("Eric", "Evans"));

            //When
            var eventStream = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, from: DateTime.UtcNow.AddDays(-1).AddHours(-2), to: DateTime.Now);

            //ThenIWillExpect
            eventStream.Count.Should().Be(3);
        }


        [Fact]
        public async Task Given_ThereAreEventsToEventStream_When_LoadTheFirst3EventsFromPosition2_Then_Only3EventsStartFromEvent3WillBeReturned()
        {
            //Given
            var customer = CreateCustomer("1");
            customer.ChangeName(new FullName("Greg", "Young"));
            customer.ChangeName(new FullName("Martin", "Fowler"));
            customer.ChangeName(new FullName("Eric", "Evans"));
            customer.ChangeName(new FullName("Kent", "Beck"));

            var toBeAppendEvents = new List<AppendEventDto>();
            foreach (var @event in customer.DeQueueDomainEvents())
            {
                var secondEventViewModel = AppendEventDto.Version1(payload: @event);
                toBeAppendEvents.Add(secondEventViewModel);
            }
            //Append 5 events
            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, toBeAppendEvents);

            //When
            var eventStream = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity
            , new EventStreamPosition(2 ,2 ),
                EventStreamVersion.Any
                , maxCount: 3);

            //ThenIWillExpect
            eventStream.Count.Should().Be(3);
        }


        [Fact]
        public async Task Load_TheSpecifiedEventStreamVersionIsCorrect_AllEventsOfTheSpecifiedVersionWillBeReturned()
        {
            var customer = CreateCustomer("1");

            var firstEvent = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, firstEvent);


            customer.ChangeName(new FullName("Eric", "Evans"));

            var secondEvent = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, secondEvent);

            var eventStream = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, EventStreamPosition.AtStart()
                , version: (EventStreamVersion)2);

            eventStream.Count.Should().Be(1);
            eventStream.Version.Should().Be(2);
        }

        [Fact]
        public async Task Append_AfterApendingAnEvent_EventStreamCurrenPositionWillBeIncrementedAndItsValueWillBeSetOnThatEvent()
        {
            var customer = CreateCustomer("1");

            var firstEvent = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, firstEvent);
            var eventStream = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, EventStreamPosition.AtStart());
            
            //first event should be appended at commitPosition 0
            eventStream.Events.Single().GlobalCommitPosition.Should().Be(0);

            customer.ChangeName(new FullName("Eric", "Evans"));
            var secondEvent = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, secondEvent);
            eventStream = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, EventStreamPosition.AtStart());

            eventStream.Events.First(e => e.EventType == typeof(CustomerNameIsChangedEvent).AssemblyQualifiedName).GlobalCommitPosition.Should().Be(1);
        }

        [Fact]
        public async Task Append_TheEventStreamHsaNotBeenCreated_EventStreamWillBeCreatedImplicitlyAndTheEventAppended()
        {
            var customer = CreateCustomer("1");

            var eventViewModel = AppendEventDto.Version1(payload: customer.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, eventViewModel);
            var events = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, EventStreamPosition.AtStart());

            events.Events.Should().HaveCount(1);
            events.Events.Single().EventType.Should().BeEquivalentTo(typeof(ANewCustomerIsCreatedEvent).AssemblyQualifiedName);
        }

        [Fact]
        public async Task Append_ACollectionOfEventsAppending_AllEventsWillBeAppendedSequentiallyAndEventStreamVersionWillBeIncremented()
        {
            var customer = CreateCustomer("1");

            customer.ChangeName(new FullName("Greg", "Young"));
            ICollection<AppendEventDto> toBeAppendedEvents = new List<AppendEventDto>();
            foreach (var @event in customer.DeQueueDomainEvents())
            {
                var eventViewModel = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: @event);
                toBeAppendedEvents.Add(eventViewModel);
            }

            await _inMemoryEventStore.AppendToEventStreamAsync(customer.Identity, toBeAppendedEvents);
            var events = await _inMemoryEventStore.LoadEventStreamAsync(customer.Identity, EventStreamPosition.AtStart());

            events.Events.Should().HaveCount(2);
            events.Events.Should().Contain(e => e.EventType == typeof(ANewCustomerIsCreatedEvent).AssemblyQualifiedName);
            events.Events.Should().Contain(e => e.EventType == typeof(CustomerNameIsChangedEvent).AssemblyQualifiedName);
        }

        [Fact]
        public async Task Append_TheSecondInstanceOfAnAggregatesEventsIsAppending_ANewEventStreamWillBeCreatedForEachInstanceForeEachInstanceOneEventStredneppamShouldBeCreated()
        {
            var customer1 = CreateCustomer("1");
            var eventViewModel1 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer1.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);
            var eventStream1 = await _inMemoryEventStore.LoadEventStreamAsync(customer1.Identity, EventStreamPosition.AtStart());
            eventStream1.Events.Should().HaveCount(1);

            var customer2 = CreateCustomer("2");

            var eventViewModel2 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer2.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer2.Identity, eventViewModel2);
            var eventStream2 = await _inMemoryEventStore.LoadEventStreamAsync(customer2.Identity, EventStreamPosition.AtStart());
            eventStream2.Events.Should().HaveCount(1);
        }

        [Fact]
        public async Task Append_AfterSuccessfulAppendingOfAnEvent_EventStreamVersionWillBeIncremented()
        {
            var customer1 = CreateCustomer("1");

            var eventViewModel1 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer1.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);
            var events = await _inMemoryEventStore.LoadEventStreamAsync(customer1.Identity, EventStreamPosition.AtStart());

            events.Version.Should().Be(1);

            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);
            events = await _inMemoryEventStore.LoadEventStreamAsync(customer1.Identity, EventStreamPosition.AtStart());

            events.Version.Should().Be(2);
        }

        [Fact]
        public async Task Delete_EventStrea()
        {
            //Given
            var customer1 = CreateCustomer("1");

            var eventViewModel1 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer1.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);

            //When
            customer1.Delete();
            eventViewModel1 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer1.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);

            //then
            var events = await _inMemoryEventStore.LoadEventStreamAsync(customer1.Identity, EventStreamPosition.AtStart());
            events.Events.Should().BeEmpty();
        }

        [Fact]
        public async Task Given_EventStreamIsMarkAsDeleted_When_EventIsAppendinggToIt_Then_ExceptionWillBeThrown()
        {
            //Given
            var customer1 = CreateCustomer("1");

            var eventViewModel1 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer1.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);

            //When
            customer1.Delete();
            eventViewModel1 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer1.DeQueueDomainEvents().Single());
            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);

            //then
           await Assert.ThrowsAsync<AppendingToDeletedEventStreamException>(() =>
            _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1)
            );

        }

        [Fact]
        public async Task EventStreamMetaData()
        {
            //Given
            var customer1 = CreateCustomer("1");

            var eventViewModel1 = AppendEventDto.Version1(globalUniqueEventId: Guid.NewGuid().ToString(), payload: customer1.DeQueueDomainEvents().Single());

            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);

            //When
            var metaData = await _inMemoryEventStore.EventStreamMetaDataAsync(customer1.Identity);

            //then
            metaData.Version.Should().Be(1);
            metaData.Positions.Should().Be(Position.At(1));
            metaData.MasrkAsDeleted.Should().BeFalse();


            await _inMemoryEventStore.AppendToEventStreamAsync(customer1.Identity, eventViewModel1);

            //When
            metaData = await _inMemoryEventStore.EventStreamMetaDataAsync(customer1.Identity);

            //then
            metaData.Version.Should().Be(2);
            metaData.Positions.Should().Be(Position.At(2));
            metaData.MasrkAsDeleted.Should().BeFalse();
        }


        private Customer CreateCustomer(string id)
            => new(new CustomerId(id), new FullName("Jon", "Doe"));
    }
}