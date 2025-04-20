using System.Collections.Generic;
using Quantum.Domain;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing.Versioning;

public abstract class IDomainEventTransformer<TFrom> 
{
    public abstract List<IsADomainEvent> Transform(TFrom from);
}