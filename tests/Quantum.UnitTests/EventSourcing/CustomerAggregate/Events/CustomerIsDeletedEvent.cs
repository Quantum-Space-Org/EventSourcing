namespace Quantum.UnitTests.EventSourcing.CustomerAggregate.Events
{
    public class CustomerIsDeletedEvent : DeleteEvent
    {
        public CustomerIsDeletedEvent() :base("")
        
     {
        }

        public CustomerIsDeletedEvent(string id)
            : base(id)
        {
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return AggregateId;
        }
    }
}