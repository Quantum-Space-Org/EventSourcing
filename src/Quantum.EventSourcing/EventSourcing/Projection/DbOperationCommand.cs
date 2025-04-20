using System.Threading.Tasks;

namespace Quantum.EventSourcing.Projection;

public abstract class DbOperationCommand
{
    public abstract Task Execute();
}