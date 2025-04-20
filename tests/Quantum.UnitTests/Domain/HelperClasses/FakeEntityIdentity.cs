namespace Quantum.UnitTests.Domain.HelperClasses
{
    public class FakeEntityIdentity : IsAnIdentity<FakeEntityIdentity>
    {
        private readonly long _id;

        public FakeEntityIdentity(long id)
        {
            _id = id;
        }
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return _id;
        }
    }
}