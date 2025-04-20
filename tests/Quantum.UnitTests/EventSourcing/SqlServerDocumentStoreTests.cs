using System.Data.Common;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Quantum.DataBase;
using Quantum.EventSourcing;
using Quantum.EventSourcing.Projection;
using Quantum.EventSourcing.SqlServerDocumentStore;
using Quantum.EventSourcing.Subscriber;
using Quantum.Resolver;
using Quantum.Resolver.ServiceCollection;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Projector;
using Quantum.UnitTests.EventSourcing.TestSpecificClasses;

namespace Quantum.UnitTests.EventSourcing
{
    public class SqlServerDocumentStoreTests
    {
        [Fact]
        // very fragile test
        public async Task ResolveProjectorInAssembly()
        {
            var assembly = GetType().Assembly;

            List<Type> projectorTypes = assembly.ResolveChildrenOf(typeof(ImAProjector));
            projectorTypes.Count.Should().Be(2);
        }

        [Fact]
        public async Task ResolveOnMethodsWhenDomainEventGrandFatherIsSDomainEvent()
        {
            Ledger ledger = CreateLedger(GetType().Assembly);

            List<Type> types = ledger.WhoAreInterestedIn(typeof(CustomerIsDeletedEvent));
            types.Count.Should().Be(1);
            types.Single().Should().Be(typeof(CustomerViewModelProjector));
        }

        [Fact]
        public async Task SuccessfullyResolvedFilterEventsImInterestedIn()
        {
            var type = typeof(CustomerViewModelProjector);

            List<Type> types = type.InterestIn();

            types.Count.Should().Be(3);
            types.Should().Contain(typeof(ANewCustomerIsCreatedEvent));
        }


        [Fact]
        public async Task LedgerShouldSuccessfullyResolveProjectors()
        {
            Ledger ledger = CreateLedger(GetType().Assembly);

            List<Type> types = ledger.WhoAreInterestedIn(typeof(ANewCustomerIsCreatedEvent));
            types.Count.Should().Be(1);
            types.Single().Should().Be(typeof(CustomerViewModelProjector));
        }


        [Fact]
        public async Task ShoouldSuccessfullyStoreStateOfTheProjector()
        {
            Ledger ledger = CreateLedger(GetType().Assembly);

            ServiceCollection serviceCollection = new();
            serviceCollection.AddScoped<CustomerViewModelProjector>();
            serviceCollection.AddScoped<IDocumentStore, SqlServerDocumentStore>();
            serviceCollection.AddSingleton(sp => new DbContextOptionsBuilder<QuantumDbContext>()
                    .UseSqlite(CreateInMemoryDatabase()).Options);
            serviceCollection.AddScoped<QuantumDbContext, MyQuantumDbContext>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IResolver resolver = new ServiceCollectionResolver(serviceProvider);

            var documentStore = resolver.Resolve<IDocumentStore>();
            var quantumDbContext = resolver.Resolve<QuantumDbContext>();

            var notifyProjectorsSubscriber = new NotifyProjectorsSubscriber(ledger, resolver, NullLogger<NotifyProjectorsSubscriber>.Instance, quantumDbContext);

            notifyProjectorsSubscriber.AnEventAppended(
                new EventViewModel
                {
                    EventType = typeof(ANewCustomerIsCreatedEvent).AssemblyQualifiedName,
                    Version = 1,
                    Payload = new ANewCustomerIsCreatedEvent("1", "Greg", "Young"),
                    GlobalCommitPosition = 1,
                    PositionAtItsOwnEventStream = 1,
                    Metadata = "",
                    EventId = "1"
                });

            var projector = resolver.Resolve<CustomerViewModelProjector>();
            projector.IsCalled().Should().BeTrue();

            var document = await documentStore.Fetch<CustomerViewModel>(o => o.Id == "1");
            document.FirstName.Should().BeEquivalentTo("Greg");
            document.LastName.Should().BeEquivalentTo("Young");
        }

        [Fact]
        public async Task ShouldSuccessfullyStoreCheckpoint()
        {
            var documentStore = new SqlServerDocumentStore(new MyQuantumDbContext(
                new DbContextOptionsBuilder<QuantumDbContext>()
                    .UseSqlite(CreateInMemoryDatabase())
                    ));

            await documentStore.StoreCheckpoint(new CheckPoint(nameof(NotifyProjectorsSubscriber), 1, 1));
            var checkpoint = await documentStore.GetCheckPoint(nameof(NotifyProjectorsSubscriber));
            checkpoint.Should().BeEquivalentTo(new CheckPoint(name: nameof(NotifyProjectorsSubscriber), commitPosition: 1, preparePosition: 1, version: 1));

            //Update checkpoint. Move to posiiton 2
            await documentStore.StoreCheckpoint(new CheckPoint(nameof(NotifyProjectorsSubscriber), 2, 2));

            //Verify
            checkpoint = await documentStore.GetCheckPoint(nameof(NotifyProjectorsSubscriber));
            checkpoint.Should().BeEquivalentTo(new CheckPoint(name: nameof(NotifyProjectorsSubscriber), commitPosition: 2, preparePosition: 2, version: 1));
        }

        [Fact]
        public async Task ShouldSuccessfullyStoreCheckpoint_IfServicesAreResolvedFromContaier()
        {
            var servicewCllection = new ServiceCollection();

            servicewCllection.AddSingleton(new Ledger(typeof(NotifyProjectorsSubscriber).Assembly));

            servicewCllection.AddSingleton<ILogger<NotifyProjectorsSubscriber>>(a => NullLogger<NotifyProjectorsSubscriber>.Instance);


            servicewCllection.AddSingleton<IResolver>(sp => new ServiceCollectionResolver(sp));

            servicewCllection.AddSingleton<ICatchUpSubscriber, NotifyProjectorsSubscriber>();

            var resolver = servicewCllection.BuildServiceProvider();

            var notifier = resolver.GetRequiredService<ICatchUpSubscriber>();

            ((NotifyProjectorsSubscriber)notifier).SaveCheckPointAsync(commitPosition: 1, preparePosition: 1, version: 1);
            ((NotifyProjectorsSubscriber)notifier).SaveCheckPointAsync(commitPosition: 1, preparePosition: 1, version: 1);
        }


        private Ledger CreateLedger(Assembly assembly)
            => new(assembly);

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
        }
    }
}