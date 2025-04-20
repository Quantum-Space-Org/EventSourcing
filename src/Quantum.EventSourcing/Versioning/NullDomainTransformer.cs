using System.Collections.Generic;
using Quantum.Domain;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing.Versioning;

public class NullDomainTransformer : IDomainEventTransformer<IsADomainEvent>
{
    public static NullDomainTransformer New() => new();
    public override List<IsADomainEvent> Transform(IsADomainEvent from)
    {
        return new List<IsADomainEvent>{from};
    }
}