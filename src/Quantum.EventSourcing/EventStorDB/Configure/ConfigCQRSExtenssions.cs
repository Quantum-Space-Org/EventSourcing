using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;
using Quantum.EventSourcing;
using Quantum.EventSourcing.EventStoreDB;

namespace Quantum.Dispatcher.Configurator
{
    public static class ConfigEventStoreExtenssions
    {
        public static ConfigEventStoreBuilder ConfigEventStore(this QuantumServiceCollection collection)
        {
            return new ConfigEventStoreBuilder(collection);
        }
    }

    public class ConfigEventStoreBuilder
    {
        private readonly QuantumServiceCollection _quantumServiceCollection;

        public ConfigEventStoreBuilder(QuantumServiceCollection collection)
        {
            _quantumServiceCollection = collection;
        }


        public QuantumServiceCollection and()
        {
            return _quantumServiceCollection;
        }

        public ConfigEventStoreBuilder RegisterEventStoreAsTransient<T>()
            where T : class, IEventStore
        {
            _quantumServiceCollection.Collection.AddTransient<IEventStore, T>();
            return this;
        }

        public ConfigEventStoreBuilder RegisterEventStoreAsSingleton<T>()
            where T : class, IEventStore
        {
            _quantumServiceCollection.Collection.AddSingleton<IEventStore, T>();
            return this;
        }

        public ConfigEventStoreBuilder RegisterEventStoreConfiguration(EventStoreDbConfig eventStoreDbConfig)
        {
            _quantumServiceCollection.Collection.AddSingleton(eventStoreDbConfig);
            return this;
        }
    }
}
