using Quantum.EventSourcing;

namespace Quantum.UnitTests.EventSourcing.CustomerAggregate.Projector
{
    public class CustomerViewModel : ViewModelBase
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}