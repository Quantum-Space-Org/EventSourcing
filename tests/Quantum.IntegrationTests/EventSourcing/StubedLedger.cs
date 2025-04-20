using System;
using System.Collections.Generic;
using Quantum.EventSourcing;

namespace Quantum.IntegrationTests.EventSourcing
{
    public class StubedLedger : ILedger
    {
        public List<Type> WhoAreInterestedIn(Type type)
        {
            return new List<Type>
            {
                typeof(MySubscriber)
            };
        }
    }
}