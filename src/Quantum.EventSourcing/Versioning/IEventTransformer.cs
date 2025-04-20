using System.Collections.Generic;
using Quantum.Domain;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing.Versioning;

public interface IEventTransformer<in TFrom>
{
    ICollection<IsADomainEvent> Transform(TFrom from);
}