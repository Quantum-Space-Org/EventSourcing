namespace Quantum.UnitTests.Domain.HelperClasses
{
    public class FakeValueObject : IsAValueObject<FakeValueObject>
    {
        private readonly string _value;

        public FakeValueObject(string value)
        {
            _value = value;
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return _value;
        }
    }
}