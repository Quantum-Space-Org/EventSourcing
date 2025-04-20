using System;
using System.Collections.Generic;
using Quantum.Resolver;

namespace Quantum.IntegrationTests;

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

    public static IResolver WhichReturn(StubedDomainEventTransformer stubedDomainEventTransformer)
    {
        return new StubedResolver
        {
            _component = stubedDomainEventTransformer,
        };
    }
}