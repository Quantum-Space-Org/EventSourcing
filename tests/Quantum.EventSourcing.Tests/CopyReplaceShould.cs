using FluentAssertions;
using Quantum.Core;
using Quantum.Domain;
using Quantum.EventSourcing.Tests.DomainEventApplier;
using Quantum.EventSourcing.Tests.TestSpecificClasses;
using Quantum.EventSourcing.Versioning;

namespace Quantum.EventSourcing.Tests;

public class CopyReplaceShould
{
    private IEventStoreVerioner _eventStoreVerioner;
    private readonly InMemoryEventStore.InMemoryEventStore _eventStore;

    public CopyReplaceShould()
    {
        _eventStore = new InMemoryEventStore.InMemoryEventStore(new DateTimeProvider());

        _eventStoreVerioner = new EventStoreVerioner(_eventStore, null);
    }

    [Fact]
    public async Task copyAnEmptyEventStream_exceptionWillBeThrown()
    {
        Func<Task> action = async () => await _eventStoreVerioner.CopyAndReplaceEventStream(from: new AggregateId("streamId"), to: new AggregateId("streamId_new"));

        await action.Should().ThrowAsync<CopyAnEmptyOrNotExistEventStreamException>();
    }

    [Fact]
    public async Task successfullyCopyAnEventStreamToAnotherStream()
    {
        var streamId = new AggregateId("streamId");
        var newStreamId = new AggregateId("streamId_new");

        await _eventStore.AppendToEventStreamAsync(streamId, new List<AppendEventDto>
        {
            AppendEventDto.Version1(Events.Event1()),
            AppendEventDto.Version1(Events.Event2()),
            AppendEventDto.Version1(Events.Event3()),
            AppendEventDto.Version1(Events.Event4()),
        });

        await _eventStoreVerioner.CopyAndReplaceEventStream(from: streamId, to: newStreamId);

        // Assert new event stream 
        var pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(newStreamId, EventStreamPosition.AtStart());
        pagedEventStreamViewModel.Count.Should().Be(4);
    }

    [Fact]
    public async Task whenLoadingFromAnOldStream_automaticallyRedirectedToNewStream()
    {
        // Context
        var streamId = new AggregateId("streamId");
        var newStreamId = new AggregateId("streamId_new");

        await _eventStore.AppendToEventStreamAsync(streamId, new List<AppendEventDto>
        {
            AppendEventDto.Version1(Events.Event1()),
            AppendEventDto.Version1(Events.Event2()),
            AppendEventDto.Version1(Events.Event3()),
        });

        await _eventStoreVerioner.CopyAndReplaceEventStream(from: streamId, to: newStreamId);

        // Action
        var event4 = Events.Event4();
        await _eventStore.AppendToEventStreamAsync(newStreamId, new List<AppendEventDto>
        {
            AppendEventDto.Version1(event4),
        });

        // Assert new event stream 
        var pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(newStreamId, EventStreamPosition.AtStart());
        pagedEventStreamViewModel.Count.Should().Be(4);

        // assert old event stream
        pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(streamId, EventStreamPosition.AtStart());
        pagedEventStreamViewModel.Payloads.Should().Contain(event4);
    }

    [Fact]
    public async Task successfullyDeleteOldStreamAfterCooyingToNewStream()
    {
        var streamId = new AggregateId("streamId");
        var newStreamId = new AggregateId("streamId_new");

        await _eventStore.AppendToEventStreamAsync(streamId, new List<AppendEventDto>
        {
            AppendEventDto.Version1(Events.Event1())
        });

        await _eventStoreVerioner.CopyAndReplaceEventStream(from: streamId, to: newStreamId, deleteOldStream: true);

        // Assert new event stream 
        var pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(streamId, EventStreamPosition.AtStart());
        pagedEventStreamViewModel.Count.Should().Be(0);
    }

    [Fact]
    public async Task successfullyTransformAndCopyEvents()
    {
        // Context
        var streamId = new AggregateId("streamId");
        var newStreamId = new AggregateId("streamId_new");
        await _eventStore.AppendToEventStreamAsync(streamId, new List<AppendEventDto>
        {
            AppendEventDto.Version1(Events.Event1())
        });


        var expectedName = "TransformedName";
        var expectedName2 = "TransformedName2";

        var eventTransformerRegistrar = new EventTransformerRegistrar(StubedResolver.WhichReturn(
            DomainEventTransformerStubbed.Which(e=>new List<IsADomainEvent>
                {
                   new Events.DomainEvent(e.AggregateId , expectedName),
                   new Events.DomainEvent(e.AggregateId , expectedName2),
                })));

        eventTransformerRegistrar.Register(Events.Event1().GetType(), typeof(DomainEventTransformerStubbed));
        
        var eventStoreCopyReplacer = new EventStoreVerioner(_eventStore, eventTransformerRegistrar);
        
        // Action
        await eventStoreCopyReplacer.CopyTransformAndReplaceEventStream(from: streamId, to: newStreamId, deleteOldStream: false);
        
        // Outcome
        var pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(newStreamId, EventStreamPosition.AtStart());
        pagedEventStreamViewModel.Count.Should().Be(2);

       ((Events.DomainEvent) pagedEventStreamViewModel.Payloads.First()).Name.Should().Be(expectedName);
       ((Events.DomainEvent) pagedEventStreamViewModel.Payloads.Last()).Name.Should().Be(expectedName2);
    }


    [Fact]
    public async Task migratedFromEvent()
    {
        // Context
        var expectedName = "nameTransformed";

        var resolver = StubedResolver.WhichReturn(StubbedDomainEventTransformer.WhichTransformNameTo(expectedName));
        var event1 = Events.Event1();

        IEventTransformerRegistrar eventTransformerRegistrar = new EventTransformerRegistrar(resolver);
        eventTransformerRegistrar.Register(typeof(Events.DomainEvent), typeof(StubbedDomainEventTransformer));

        // Action
        var transformer = eventTransformerRegistrar.GetTransformerOf(event1);
        var transformedEvent = transformer.Transform(event1);
        var domainEvents = ((List<IsADomainEvent>)transformedEvent);

        // Output
        transformedEvent.Should().NotBeNull();

        var isADomainEvent = domainEvents
            .Single();
        ((Events.DomainEvent)isADomainEvent).Name.Should().BeEquivalentTo(expectedName);
    }

    //private IEventTransformerRegistrar CreateRegistrarWhichReturnStubbedDomainEventTransformer(string expectedTransformedName)
    //{
    //    IResolver resolver = StubedResolver.WhichReturn(new StubbedDomainEventTransformer());
    //    IEventTransformerRegistrar eventTransformerRegistrar = new EventTransformerRegistrar(resolver);
    //    eventTransformerRegistrar.Register(typeof(Events.DomainEvent), typeof(StubbedDomainEventTransformer));
    //    var transformer = eventTransformerRegistrar.GetTransformerOf(Events.Event1());
    //    transformer.Should().NotBeNull();
    //}
}