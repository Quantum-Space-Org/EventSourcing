namespace Quantum.UnitTests.ProjectDomainEventTests;

public class PersonNameIsChangedDomainEvent : IsADomainEvent
{
    public string NewFirstName { get; }

    public PersonNameIsChangedDomainEvent(string aggregateId , string newFirstName) : base(aggregateId)
    {
        NewFirstName = newFirstName;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return AggregateId;
    }
}