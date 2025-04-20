using Quantum.Resolver;

namespace Quantum.EventSourcing.Tests.TestSpecificClasses;

public class StubedResolver : IResolver
{
    private object _component;

    public T Resolve<T>()
    {
        throw new NotImplementedException();
    }

    public object Resolve(Type type)
    {
        return _component;
    }

    public IEnumerable<T> ResolveAll<T>()
    {
        throw new NotImplementedException();
    }

    
    public static IResolver WhichReturn(object component)
    {
        return new StubedResolver
        {
            _component = component,
        };
    }
    

}