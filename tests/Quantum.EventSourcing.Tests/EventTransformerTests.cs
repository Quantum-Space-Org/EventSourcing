using FluentAssertions;
using Quantum.EventSourcing.Tests.DomainEventApplier;
using Quantum.EventSourcing.Tests.TestSpecificClasses;
using Quantum.EventSourcing.Versioning;

namespace Quantum.EventSourcing.Tests;

public class EventTransformerTests
{
    [Fact]
    public void transformEvent()
    {
        // Context
        var expectedName = "nameTransformed";

        var resolver = StubedResolver.WhichReturn(StubedDomainEventTransformer.WhichTransformNameTo(expectedName));
        var event1 = Events.Event1();

        IEventTransformerRegistrar eventTransformerRegistrar = new EventTransformerRegistrar(resolver);
        eventTransformerRegistrar.Register(typeof(Events.DomainEvent), typeof(StubedDomainEventTransformer));

        // Action
        var transformer = eventTransformerRegistrar.GetTransformerOf(Type.GetType(event1.GetType().AssemblyQualifiedName));

        var transformedEvent = ((StubedDomainEventTransformer)transformer).Transform(event1);
        
        // Output
        Assert.NotNull(transformedEvent);
        
        ((Events.DomainEvent)transformedEvent.Single()).Name.Should().BeEquivalentTo(expectedName);
    }

    [Fact]
    public void registerTransformers()
    {
        // Context
        var resolver = StubedResolver.WhichReturn(new StubedDomainEventTransformer());
        IEventTransformerRegistrar eventTransformerRegistrar = new EventTransformerRegistrar(resolver);
        eventTransformerRegistrar.Register(typeof(Events.DomainEvent), typeof(StubedDomainEventTransformer));

        // Action
        var transformer = eventTransformerRegistrar.GetTransformerOf(Type.GetType(typeof(Events.DomainEvent).AssemblyQualifiedName));

        // Output
        Assert.NotNull(transformer);
    }

    [Fact]
    public void registerTransformersInAnAssembly()
    {
        var resolver = StubedResolver.WhichReturn(new StubedDomainEventTransformer());
        IEventTransformerRegistrar eventTransformerRegistrar = new EventTransformerRegistrar(resolver);

        eventTransformerRegistrar.Register(assembly: typeof(Events.DomainEvent).Assembly);

        var transformer = eventTransformerRegistrar.GetTransformerOf(Type.GetType(typeof(Events.DomainEvent).AssemblyQualifiedName));

        Assert.NotNull(transformer);

        Assert.True(transformer is StubedDomainEventTransformer);
    }

    [Fact]
    public void tryToGetTransformerOfAnEventWhichWasNotRegistered()
    {
        // Context
        IEventTransformerRegistrar eventTransformerRegistrar = new EventTransformerRegistrar(null);

        // Action
        var transformer = eventTransformerRegistrar.GetTransformerOf(Type.GetType(typeof(Events.DomainEvent).AssemblyQualifiedName));

        // Output
        Assert.NotNull(transformer);
        Assert.True(transformer is NullDomainTransformer);
    }
}