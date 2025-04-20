using System;
using System.Collections.Generic;

namespace Quantum.EventSourcing;

public interface ILedger
{
    List<Type> WhoAreInterestedIn(Type type);
}