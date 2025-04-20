using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.Resolver;

namespace Quantum.EventSourcing.Versioning.Config;

public static class EventSourcingVersioningConfiguratorExtensions
{
    public static ConfigEventSourcingVersioningBuilder ConfigEventSourceVersioning(this QuantumServiceCollection collection)
    {
        return new ConfigEventSourcingVersioningBuilder(collection);
    }
}

public class ConfigEventSourcingVersioningBuilder
{
    private readonly QuantumServiceCollection _collection;

    public ConfigEventSourcingVersioningBuilder(QuantumServiceCollection collection)
        => _collection = collection;

    public ConfigEventSourcingVersioningBuilder RegisterTransformers(params Assembly[] assemblies)
    {
        _collection.Collection.AddSingleton(sp =>
        {
            var resolver = sp.GetRequiredService<IResolver>();
            IEventTransformerRegistrar registrar = new EventTransformerRegistrar(resolver);
            registrar.Register(assemblies);
            return registrar;
        });

        return this;
    }

    public ConfigEventSourcingVersioningBuilder RegisterTransformers(
        Dictionary<Type, Type> dictionary)
    {
        _collection.Collection.AddSingleton(sp =>
        {
            var resolver = sp.GetRequiredService<IResolver>();
            IEventTransformerRegistrar registrar = new EventTransformerRegistrar(resolver);

            foreach (var d in dictionary)
            {
                registrar.Register(d.Key, d.Value);
            }

            return registrar;
        });

        return this;
    }

    public ConfigEventSourcingVersioningBuilder RegisterEventStoreCopyReplacer<T>()
        where T : class, IEventStoreVerioner
    {
        _collection.Collection.AddTransient<IEventStoreVerioner, T>();
        return this;
    }

    public ConfigEventSourcingVersioningBuilder RegisterDefaultEventStoreCopyReplacer()
    {
        RegisterEventStoreCopyReplacer<EventStoreVerioner>();
        return this;
    }

    public QuantumServiceCollection and() 
        => _collection;
}