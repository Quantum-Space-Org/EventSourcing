using System;
using System.Collections.Generic;
using System.Reflection;
using Quantum.EventSourcing.Versioning;

namespace Quantum.IntegrationTests;

public class NullEventTransformerRegistrar : IEventTransformerRegistrar
{
    public static IEventTransformerRegistrar New() => new NullEventTransformerRegistrar();
  
    public object GetTransformerOf(Type eventType)
    {
        throw new NotImplementedException();
    }

    IEventTransformer<T> IEventTransformerRegistrar.GetTransformerOf<T>(T eventType)
    {
        throw new NotImplementedException();
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