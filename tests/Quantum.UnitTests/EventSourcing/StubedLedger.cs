using Quantum.EventSourcing;

namespace Quantum.UnitTests.EventSourcing
{
    public class StubLedger : ILedger
    {
        private Type _type;

        public static ILedger WhichReturn(Type type)
        {
            return new StubLedger { _type = type };
        }

        public List<Type> WhoAreInterestedIn(Type type)
        {
            return type == typeof(MyDomainEvent) ? new List<Type> { _type } : new List<Type>();
        }
    }

    public class MyDomainEvent:IsADomainEvent
    {
        public MyDomainEvent(string aggregateId) : base(aggregateId)
        {
        }
    }

    public class StubedLedger : ILedger
    {
        public List<Type> WhoAreInterestedIn(Type type)
        {
            return new List<Type>
            {
                typeof(MyProjector)
            };
        }
    }
}