using System;
using System.Linq;
using Quantum.Domain.Messages.Event;
using Quantum.Core;

namespace Quantum.EventSourcing.Projection;

public class Im
{
    public static Func<IsADomainEvent, bool> InterestedInAllEvents()
        => e => true;

    public static Func<IsADomainEvent, bool> InterestedIn(params Type[] types)
        => e => types != null
                &&
                types.Any(e.IsOfType);
}