namespace Quantum.UnitTests.EventSourcing.CustomerAggregate.Events
{
    public class CustomerNameIsChangedEvent : IsADomainEvent
    {
        public string FirstName { get; }
        public string LastName { get; }

        public CustomerNameIsChangedEvent(string aggregateId, string firstName, string lastName)
        : base(aggregateId)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return AggregateId;
            yield return FirstName;
            yield return LastName;
        }
    }
}