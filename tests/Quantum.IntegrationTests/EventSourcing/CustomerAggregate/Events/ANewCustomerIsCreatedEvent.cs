using System.Collections.Generic;

namespace Quantum.IntegrationTests.EventSourcing.CustomerAggregate.Events
{
    public class ANewCustomerIsCreatedEvent : IsADomainEvent
    {
        public string CustomerId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public ANewCustomerIsCreatedEvent(string customerId, string firstName, string lastName)
        : base(customerId)
        {
            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return CustomerId;
            yield return FirstName;
            yield return LastName;
        }
    }
}