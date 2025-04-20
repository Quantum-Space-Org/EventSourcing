using System.Reflection;
using Quantum.EventSourcing.Versioning;

namespace Quantum.EventSourcing.Tests.TestSpecificClasses;

public class NullEventTransformerRegistrar : IEventTransformerRegistrar
{
    public static IEventTransformerRegistrar New() => new NullEventTransformerRegistrar();
   
    public object GetTransformerOf(Type eventType)
    {
        return default;
    }

    public IEventTransformer<T> GetTransformerOf<T>(T eventType)
    {
        return default;
    }

    public void Register(Type type, Type domainEventTransformerType)
    {

    }

    public void Register(Dictionary<Type, Type> dictionary)
    {

    }

    public void Register(Assembly assembly)
    {

    }

    public void Register(params Assembly[] assembly)
    {
    }
}