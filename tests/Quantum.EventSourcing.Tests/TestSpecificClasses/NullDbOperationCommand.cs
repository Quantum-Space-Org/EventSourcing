using Quantum.EventSourcing.Projection;

namespace Quantum.EventSourcing.Tests.TestSpecificClasses
{
    internal class NullDbOperationCommand : DbOperationCommand
    {
        public static DbOperationCommand Instance
        {
            get
            {
                return new NullDbOperationCommand();
            }
        }

        public override Task Execute()
        {
            return Task.CompletedTask;
        }
    }
}