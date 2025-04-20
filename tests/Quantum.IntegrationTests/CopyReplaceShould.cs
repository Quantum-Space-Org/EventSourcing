using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Quantum.Core;
using Quantum.EventSourcing;
using Quantum.EventSourcing.EventStoreDB;
using Quantum.EventSourcing.Versioning;
using Quantum.IntegrationTests.EventSourcing;
using Xunit;
using StubbedResolver = Quantum.IntegrationTests.StubedResolver;

namespace Quantum.IntegrationTests;

public class CopyReplaceShould : IClassFixture<EventStoreDbEventStoreTestsBase>
{
    private readonly IEventStore _eventStore;
    private IEventStoreVerioner _eventStoreVerioner;

    public CopyReplaceShould(EventStoreDbEventStoreTestsBase c)
    {
        var logger = new NullLogger<EventStoreDbEventStore>();

        _eventStore = new EventStoreDbEventStore(c.EventStoreDbConfig, logger);
        _eventStoreVerioner = new EventStoreVerioner(_eventStore, NullEventTransformerRegistrar.New());
    }

    [Fact]
    public async Task copyAnEmptyEventStream_exceptionWillBeThrown()
    {
        var action = async () => await _eventStoreVerioner.CopyAndReplaceEventStream(from: new AggregateId("streamId"), to: new AggregateId("streamId_new"));

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
        const string expectedTransformedName = "newName";

        await _eventStore.AppendToEventStreamAsync(streamId, new List<AppendEventDto>
        {
            AppendEventDto.Version1(Events.Event1())
        });

        var registrar = CreateRegistrarWhichReturnStubbedDomainEventTransformer(expectedTransformedName);
        _eventStoreVerioner = new EventStoreVerioner(_eventStore, registrar);

        // Action
        await _eventStoreVerioner.CopyTransformAndReplaceEventStream(from: streamId, to: newStreamId, deleteOldStream: false);

        // Output
        var pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(newStreamId, EventStreamPosition.AtStart());
        pagedEventStreamViewModel.Count.Should().Be(1);

        ((Events.DomainEvent)pagedEventStreamViewModel.Payloads.Single()).Name.Should().Be(expectedTransformedName);
    }


    [Fact]
    public async Task migratedFromEvent()
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
        pagedEventStreamViewModel.Count.Should().Be(5 , "این تست باید خطا بخورد. ایونت مایگریتت فرام نیاز به مکانیزمی جهت اسرت شدن دارد.");
        pagedEventStreamViewModel.Payloads.First()
            .GetType()
            .Should().Be(typeof(StreamWasMigratedFromDomainEvent));
    }

    private IEventTransformerRegistrar CreateRegistrarWhichReturnStubbedDomainEventTransformer(string expectedTransformedName)
    {
        var resolver = StubedResolver.WhichReturn(StubedDomainEventTransformer.WhichTransformNameTo(expectedTransformedName));

        IEventTransformerRegistrar registrar = new EventTransformerRegistrar(resolver);
        registrar.Register(GetType().Assembly);

        return registrar;
    }
}