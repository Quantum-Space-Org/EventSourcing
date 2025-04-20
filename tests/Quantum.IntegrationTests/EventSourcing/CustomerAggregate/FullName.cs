using System.Collections.Generic;

namespace Quantum.IntegrationTests.EventSourcing.CustomerAggregate
{
    public class FullName : IsAValueObject<FullName>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public FullName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
        }
    }
}