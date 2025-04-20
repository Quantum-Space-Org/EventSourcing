using Microsoft.Extensions.DependencyInjection;
using Quantum.Configurator;

namespace Quantum.EventSourcing.Test.Configurator
{
    public static class ConfigQuantumEventSourcingTestExtensions
    {
        public static ConfigQuantumEventSourcingTestBuilder ConfigQuantumEventSourcingTest(
            this QuantumServiceCollection quantumServiceCollection)
        {
            return new ConfigQuantumEventSourcingTestBuilder(quantumServiceCollection);
        }
    }

    public class ConfigQuantumEventSourcingTestBuilder(QuantumServiceCollection quantumServiceCollection)
    {
        public ConfigQuantumEventSourcingTestBuilder RegisterEventApplierAsScoped<T>()
            where T : class, IDomainEventApplier
        {
            quantumServiceCollection.Collection.AddScoped<IDomainEventApplier, T>();
            return this;
        }

        public QuantumServiceCollection and()
        {
            return quantumServiceCollection;
        }
    }
}
