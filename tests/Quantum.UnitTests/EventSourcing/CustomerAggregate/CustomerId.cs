namespace Quantum.UnitTests.EventSourcing.CustomerAggregate
{
    public class CustomerId : IsAnIdentity<CustomerId>
    {
        public CustomerId(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}