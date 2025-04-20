
namespace Quantum.UnitTests.EventSourcing.CustomerAggregate.Commands
{
    public class CreateCustomerCommand : IsACommand
    {
        public string NationalCode;
        public string FirstName;
        public string LastName;

        public CreateCustomerCommand(string nationalCode, string firstName, string lastName)
        {
            NationalCode = nationalCode;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}