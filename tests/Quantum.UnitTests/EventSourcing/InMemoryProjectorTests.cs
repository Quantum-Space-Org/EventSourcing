using System.Data.Common;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Quantum.Core;
using Quantum.DataBase;
using Quantum.EventSourcing;
using Quantum.EventSourcing.InMemoryEventStore;
using Quantum.EventSourcing.Projection;
using Quantum.EventSourcing.SqlServerDocumentStore;
using Quantum.EventSourcing.Subscriber;
using Quantum.Resolver;
using Quantum.Resolver.ServiceCollection;
using Quantum.UnitTests.EventSourcing.CustomerAggregate;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Projector;
using Quantum.UnitTests.EventSourcing.TestSpecificClasses;

namespace Quantum.UnitTests.EventSourcing
{
    public class InMemoryProjectorTests
    {
        private readonly IEventStore _inMemoryEventStore;

        public InMemoryProjectorTests()
        {
            IDateTimeProvider dateTimeProvider = new DummyDateTimeProvider();

            _inMemoryEventStore = new InMemoryEventStore(dateTimeProvider);
        }

        private Ledger CreateLedger(Assembly assembly)
           => new(assembly);

        [Fact]
        public async void Given_Projector_When_EventAppended_Then_ProjectorWillBeNotifiedWithInitialState_And_StateWillBeSuccessfullyTransformedAndSaved()
        {
            var eventStreamId = new CustomerId("1");

            Ledger ledger = CreateLedger(GetType().Assembly);

            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(ledger);
            serviceCollection.AddScoped<CustomerViewModelProjector>();
            serviceCollection.AddScoped<IDocumentStore, SqlServerDocumentStore>();
            serviceCollection.AddSingleton(sp => new DbContextOptionsBuilder<QuantumDbContext>()
                    .UseSqlite(CreateInMemoryDatabase()).Options);
            serviceCollection.AddScoped<QuantumDbContext, MyQuantumDbContext>();
            serviceCollection.AddSingleton<NotifyProjectorsSubscriber>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IResolver resolver = new ServiceCollectionResolver(serviceProvider);

            var projector = resolver.Resolve<CustomerViewModelProjector>();

            var documentStore = resolver.Resolve<IDocumentStore>();

            ISubscriber subscriber = CreateNotifyProjectorsSubscriber(ledger, resolver, documentStore);
            await _inMemoryEventStore.SubscribeToEventStream(eventStreamId: eventStreamId, subscriber: subscriber);

            var customerNameIsChangedEvent = new CustomerNameIsChangedEvent(eventStreamId.Id, "customer1 name", "customer1 family");
            var appendEventDto = AppendEventDto.Version1(customerNameIsChangedEvent);

            await _inMemoryEventStore.AppendToEventStreamAsync(eventStreamId: eventStreamId, @event: appendEventDto);

            await Waiter.Wait(() => projector.IsCalled(), TimeSpan.FromSeconds(3));

            projector.Verify();
        }

        [Fact]
        public async void SuccessfullySaveTheStateAfterProjectionAnEvent()
        {
            Ledger ledger = CreateLedger(GetType().Assembly);

            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(ledger);
            serviceCollection.AddScoped<CustomerViewModelProjector>();
            serviceCollection.AddScoped<IDocumentStore, SqlServerDocumentStore>();
            serviceCollection.AddSingleton(sp => new DbContextOptionsBuilder<QuantumDbContext>()
                    .UseSqlite(CreateInMemoryDatabase()).Options);
            serviceCollection.AddScoped<QuantumDbContext, MyQuantumDbContext>();
            serviceCollection.AddSingleton<NotifyProjectorsSubscriber>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IResolver resolver = new ServiceCollectionResolver(serviceProvider);

            var projector = resolver.Resolve<CustomerViewModelProjector>();

            var documentStore = resolver.Resolve<IDocumentStore>();

            var subscriber = CreateNotifyProjectorsSubscriber(ledger, resolver, documentStore);

            var customerId = new CustomerId("1");

            await _inMemoryEventStore.SubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber);

            var aNewCustomerIsCreatedEvent = new ANewCustomerIsCreatedEvent(customerId.Id, "customer1 name", "customer1 family");

            //change name and family to Greg Young
            var nameIsChangedEvent = new CustomerNameIsChangedEvent(customerId.Id, "Greg", "Young");

            var appendEventDtos = new List<AppendEventDto>
            {
                AppendEventDto.Version1(aNewCustomerIsCreatedEvent),
                AppendEventDto.Version1(nameIsChangedEvent)
            };

            await _inMemoryEventStore.AppendToEventStreamAsync(eventStreamId: customerId, events: appendEventDtos);

            await Waiter.Wait(() => projector.IsCalled(), TimeSpan.FromSeconds(3));

            var inMemoryDocumentStore = resolver.Resolve<IDocumentStore>();
            var customer = await inMemoryDocumentStore.Fetch<CustomerViewModel>(o => o.Id == customerId.Id);
            customer.FirstName.Should().BeEquivalentTo("Greg");
            customer.LastName.Should().BeEquivalentTo("Young");
            customer.Id.Should().BeEquivalentTo(customerId.Id);
        }

        [Fact]
        public async void Given_ThereAre2Events_When_TheLastcheckpintIs1_Then_JustTheLastAppendedEventWillBeCatched()
        {
            var customerId = new CustomerId("1");

            Ledger ledger = CreateLedger(GetType().Assembly);

            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(ledger);
            serviceCollection.AddScoped<CustomerViewModelProjector>();
            serviceCollection.AddScoped<IDocumentStore, SqlServerDocumentStore>();
            serviceCollection.AddSingleton(sp => new DbContextOptionsBuilder<QuantumDbContext>()
                    .UseSqlite(CreateInMemoryDatabase()).Options);
            serviceCollection.AddScoped<QuantumDbContext, MyQuantumDbContext>();
            serviceCollection.AddSingleton<NotifyProjectorsSubscriber>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IResolver resolver = new ServiceCollectionResolver(serviceProvider);

            var projector = resolver.Resolve<CustomerViewModelProjector>();

            var documentStore = resolver.Resolve<IDocumentStore>();

            ICatchUpSubscriber subscriber = CreateNotifyProjectorsSubscriber(ledger, resolver, documentStore);

            var aNewCustomerIsCreatedEvent = new ANewCustomerIsCreatedEvent(customerId.Id, "customer1 name", "customer1 family");
            //change name and family to Greg Young
            var nameIsChangedEvent = new CustomerNameIsChangedEvent(customerId.Id, "Greg", "Young");

            var appendEventDtos = new List<AppendEventDto>
            {
                AppendEventDto.Version1(aNewCustomerIsCreatedEvent),
                AppendEventDto.Version1(nameIsChangedEvent)
            };

            await _inMemoryEventStore.AppendToEventStreamAsync(eventStreamId: customerId, events: appendEventDtos);

            await _inMemoryEventStore.CatchUpSubscribeToEventStream(eventStreamId: customerId, subscriber: subscriber, startFrom: Position.At(1));

            await Waiter.Wait(() => projector.IsCalled(), TimeSpan.FromSeconds(3));
            projector.VerifyThatItIsReceivesJustCustomerNameIsChangedEvent(nameIsChangedEvent);
        }


        [Fact]
        public async void Delete()
        {
            // Given
            Ledger ledger = CreateLedger(GetType().Assembly);
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(ledger);
            serviceCollection.AddScoped<CustomerViewModelProjector>();
            serviceCollection.AddScoped<IDeDuplicator , NullDeDuplicator>();
            serviceCollection.AddScoped<IDocumentStore, SqlServerDocumentStore>();
            serviceCollection.AddSingleton(sp => new DbContextOptionsBuilder<QuantumDbContext>()
                    .UseSqlite(CreateInMemoryDatabase()).Options);
            serviceCollection.AddScoped<QuantumDbContext, MyQuantumDbContext>();
            serviceCollection.AddSingleton<NotifyProjectorsSubscriber>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IResolver resolver = new ServiceCollectionResolver(serviceProvider);

            var projector = resolver.Resolve<CustomerViewModelProjector>();

            var documentStore = resolver.Resolve<IDocumentStore>();

            ICatchUpSubscriber subscriber = CreateNotifyProjectorsSubscriber(ledger, resolver, documentStore);


            var customerId = new CustomerId("1");
            var customer = new Customer(customerId, new FullName("Greg", "Young"));

            await _inMemoryEventStore.CatchUpSubscribeToAllEventStream(subscriber, EventStreamPosition.AtStart());

            var uncommitedEvents = customer.DeQueueDomainEvents().Select(e => AppendEventDto.Version1(e)).ToList();
            await _inMemoryEventStore.AppendToEventStreamAsync(eventStreamId: customerId, events: uncommitedEvents);

            await Waiter.Wait(() => projector.IsCalledWith<ANewCustomerIsCreatedEvent>(), TimeSpan.FromSeconds(30));

            var customerVM = await documentStore.Fetch<CustomerViewModel>(c => c.Id == "1");

            customerVM.Should().NotBeNull();

            // when
            customer.Delete();
            uncommitedEvents = customer.DeQueueDomainEvents().Select(e => AppendEventDto.Version1(e)).ToList();

            await _inMemoryEventStore.AppendToEventStreamAsync(eventStreamId: customerId, events: uncommitedEvents);

            await Waiter.Wait(() => projector.IsCalledWith<CustomerIsDeletedEvent>(), TimeSpan.FromSeconds(3));
            // Then
            customerVM = await documentStore.Fetch<CustomerViewModel>(c => c.Id == "1");

            customerVM.Should().BeNull();
        }

        private ICatchUpSubscriber CreateNotifyProjectorsSubscriber(Ledger ledger
            , IResolver resolver, IDocumentStore documentStore)
        {
            return new NotifyProjectorsSubscriber(ledger, resolver, NullLogger<NotifyProjectorsSubscriber>.Instance , null);

        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
        }
    }
}