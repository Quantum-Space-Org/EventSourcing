using Quantum.Domain.BusinessRule;
using Quantum.UnitTests.Domain.HelperClasses;

namespace Quantum.UnitTests.Domain
{
    public class BusinessRuleShould
    {
        string valueCanNotBeNull = "Value can not be null!";

        [Fact]
        public void Check_correctly()
        {
            var fakeEntity = CreateFakeEntity();

            var action = () => fakeEntity.CheckRule(CustomerNameShouldNotBeEmpty.WhichAlwaysRaiseExceptionWithMessage(valueCanNotBeNull));
            var result = action.Should().Throw<DomainValidationException>();
        }

        private static FakeEntity CreateFakeEntity()
        {
            var fakeEntity = new FakeEntity(new FakeEntityIdentity(1), "some value");
            return fakeEntity;
        }

        [Fact]
        public void Check_multiple_business_rules_correctly()
        {
            var fakeEntity = CreateFakeEntity();

            var action = () =>
                fakeEntity.CheckRules(
                    new List<IAmABusinessRule>
                    {
                        new CustomAmABusinessRule(),

                    CustomerNameShouldNotBeEmpty.WhichAlwaysRaiseExceptionWithMessage(valueCanNotBeNull)
                    });
            var result = action.Should().Throw<DomainValidationException>();
            
        }

        public class CustomAmABusinessRule : IAmABusinessRule
        {
            public CustomAmABusinessRule()
            {
            }

            public bool IsPassed()
            {
                return false;
            }

            public string GetViolationRuleMessage()
            {
                return "some error message";
            }
        }
    }
}
