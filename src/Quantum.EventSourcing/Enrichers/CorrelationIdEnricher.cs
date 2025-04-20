using System.Threading.Tasks;
using Quantum.CorrelationId;
using Quantum.UnitTests.EventSourcing;

namespace Quantum.EventSourcing.Enrichers;

public class CorrelationIdEnricher : IDomainEventEnricher
{
    private readonly ICorrelationId _correlationId;

    public CorrelationIdEnricher(ICorrelationId httpContextAccessor) 
        => _correlationId = httpContextAccessor;

    public Task<IsADomainEvent> Enrich(IsADomainEvent @event)
    {
        if (string.IsNullOrWhiteSpace(@event.MessageMetadata.CorrelationId))
            @event.MessageMetadata.CorrelationId = _correlationId.GetCorrelationId();

        return Task.FromResult(@event);
    }
}