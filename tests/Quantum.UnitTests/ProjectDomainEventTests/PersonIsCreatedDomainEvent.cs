namespace Quantum.UnitTests.ProjectDomainEventTests;

public class PersonIsCreatedDomainEvent : IsADomainEvent
{
    public string FirstName { get; }
    public string LastName { get; }

    public PersonIsCreatedDomainEvent(string aggregateId , string firstName, string lastName) : base(aggregateId)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return AggregateId;
    }
}