using Quantum.EventSourcing.Versioning;

namespace Quantum.EventSourcing.Tests.TestSpecificClasses;

public class DomainEventTransformerStubbed : IEventTransformer<Events.DomainEvent>
{
    private Func<Events.DomainEvent, ICollection<IsADomainEvent>> _function;

    private DomainEventTransformerStubbed(){}

    public static DomainEventTransformerStubbed Which(Func<Events.DomainEvent, ICollection<IsADomainEvent>> funct) =>
        new()
        {
            _function = funct
        };
    
    public ICollection<IsADomainEvent> Transform(Events.DomainEvent from)
    {
        return _function.Invoke(from);
    }
}


public class StubbedDomainEventTransformer : IEventTransformer<Events.DomainEvent>
{
    private string _expectedName;
    
    public ICollection<IsADomainEvent> Transform(Events.DomainEvent from) =>
        new List<IsADomainEvent>
        {
            from.NewWithName(_expectedName)
        };

    public static StubbedDomainEventTransformer WhichTransformNameTo(string expectedName) => new()
    {
        _expectedName = expectedName 
    };
}