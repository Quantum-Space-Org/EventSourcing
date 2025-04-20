using System;
using System.Collections.Generic;

namespace Quantum.IntegrationTests
{
    public class Events
    {
        public static DomainEvent Event1() => new(Guid.NewGuid().ToString(), "Event1");
        public static DomainEvent Event2() => new(Guid.NewGuid().ToString(), "Event2");
        public static DomainEvent Event3() => new(Guid.NewGuid().ToString(), "Event3");
        public static DomainEvent Event4() => new(Guid.NewGuid().ToString(), "Event4");

        public class DomainEvent : IsADomainEvent
        {
            public string Name { get; set; }
            public DomainEvent(string aggregateId, string name) : base(aggregateId)
            {
                Name = name;
            }

            public override IEnumerable<object> GetEqualityComponents()
            {
                yield return AggregateId;
            }

            public IsADomainEvent NewWithName(string newName)
                => new DomainEvent(AggregateId, newName);
        }
    }
}