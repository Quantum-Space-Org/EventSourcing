
namespace Quantum.UnitTests.TestSpecificClasses
{
    public class FakeCommandWithArabicProps : IsACommand
    {

        public string PropWithWrongChar1 { get; set; } = "ي";

        public string PropWithWrongChar2 { get; set; } = "ك";

        public string PropWithWrongChar3 { get; set; } = "اولأد";

        public string PropWithSpace { get; set; } = "  حمید بابایی  ";
    }
}