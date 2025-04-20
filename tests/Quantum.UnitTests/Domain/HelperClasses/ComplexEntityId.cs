namespace Quantum.UnitTests.Domain.HelperClasses
{
    public class ComplexEntityId : IsAnIdentity<ComplexEntityId>    {
        private readonly long _id;
        private readonly string _name;

        public ComplexEntityId(long id , string name)
        {
            _id = id;
            _name = name;
        }
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return _id;
            yield return _name;
        }
    }
}