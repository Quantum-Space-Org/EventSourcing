namespace Quantum.EventSourcing.Tests.TestSpecificClasses
{

    public class StubLedger : ILedger
    {
        private Type type;

        public StubLedger(Type type)
        {
            this.type = type;
        }

        public static ILedger WhichReturn(Type type)
        {
            return new StubLedger(type);
        }
        public List<Type> WhoAreInterestedIn(Type type)
        {
            return new List<Type> { this.type };
        }
    }

}
