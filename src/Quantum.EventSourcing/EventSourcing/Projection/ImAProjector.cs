using System.Threading.Tasks;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing.Projection;

public abstract class ImAProjector
{
    public async Task Process(IsADomainEvent @event)
    {
        var command = Transform(@event);

        await command.Execute();
    }

    public abstract DbOperationCommand Transform(IsADomainEvent @event);
}