using System.Collections.Generic;
using Quantum.EventSourcing.Versioning;

namespace Quantum.IntegrationTests;

public class StubedDomainEventTransformer : IDomainEventTransformer<Events.DomainEvent>
{
    private string _expectedName;
    public static StubedDomainEventTransformer WhichTransformNameTo(string expectedName)
        => new() { _expectedName = expectedName };

    public override List<IsADomainEvent> Transform(Events.DomainEvent from)
        => new List<IsADomainEvent>{from.NewWithName(_expectedName)};
}