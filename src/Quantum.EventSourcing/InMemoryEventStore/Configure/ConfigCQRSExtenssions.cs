using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;

namespace Quantum.EventSourcing.InMemoryEventStore.Configure
{
    public static class ConfigInMemoryEventStoreExtenssions
    {
        public static ConfigInMemoryEventStoreBuilder ConfigInMemoryEventStore(this QuantumServiceCollection collection)
        {
            return new ConfigInMemoryEventStoreBuilder(collection);
        }
    }

    public class ConfigInMemoryEventStoreBuilder
    {
        private readonly QuantumServiceCollection _quantumServiceCollection;

        public ConfigInMemoryEventStoreBuilder(QuantumServiceCollection collection)
        {
            _quantumServiceCollection = collection;
        }


        public QuantumServiceCollection and()
        {
            return _quantumServiceCollection;
        }

        public ConfigInMemoryEventStoreBuilder RegisterInMemoryEventStoreAsSingltone()
        {
            _quantumServiceCollection.Collection.AddSingleton<IEventStore, InMemoryEventStore>();
            return this;

        }
    }
}
