using Quantum.EventSourcing.Enrichers;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;

namespace Quantum.UnitTests.EventSourcing;

public class DomainEventEnricherTests
{
    [Fact]
    public async Task enrich_domain_event_with_correlation_id()
    {
        IDomainEventEnricher domainEventEnricher = new DomainEventEnricher();

        var enrich = await domainEventEnricher.Enrich(new CustomerNameIsChangedEvent("1", "Martin", "Fowler"));

        enrich.MessageMetadata
            .CorrelationId
            .Should()
            .BeEquivalentTo("Aa147893256");

        enrich.MessageMetadata
            .CreatedBy
            .Should()
            .BeEquivalentTo("Martin Fowler");

        enrich.MessageMetadata
            .ServiceName
            .Should()
            .BeEquivalentTo("Accounting");

    }
}