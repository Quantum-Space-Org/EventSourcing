using Quantum.DataBase;
using Quantum.DataBase.EntityFramework;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing;

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Projection;
using Subscriber;
using Resolver;

public class NotifyProjectorsSubscriber : ICatchUpSubscriber, ISubscriber
{
    private readonly ILedger _ledger;
    private readonly IResolver _resolver;
    private readonly ILogger<NotifyProjectorsSubscriber> _logger;
    private QuantumDbContext _quantumDbContext;
    public NotifyProjectorsSubscriber(ILedger ledger, IResolver resolver,
        ILogger<NotifyProjectorsSubscriber> logger, QuantumDbContext quantumDbContext)
    {
        _ledger = ledger;
        _resolver = resolver;
        _logger = logger;
        _quantumDbContext = quantumDbContext;
    }

    public string Name => nameof(NotifyProjectorsSubscriber);

    public void AnEventAppended(EventViewModel eventViewModel)
    {
        var eventId = new EventId(LogEventIds.ProjectingDomainEvent, eventViewModel.EventType);
        _logger.LogInformation(eventId, $"An event observed. {eventViewModel.EventType} {eventViewModel.EventId}");

        if (IsItADuplicateEventWeHaveSeenBefore(eventViewModel.EventId, eventViewModel.EventType))
        {
            return;
        }

        var @event = eventViewModel.Payload as IsADomainEvent;

        var projectorTypes = _ledger.WhoAreInterestedIn(Type.GetType(@eventViewModel.EventType));

        _logger.LogInformation(eventId, $"{projectorTypes.Count} projectors are exist that interested in {eventViewModel.EventType}");

        if (projectorTypes != null && projectorTypes.Any())
        {
            foreach (var projectorType in projectorTypes)
            {
                _logger.LogInformation(eventId, $"Start projecting {projectorType.FullName} with event type {eventViewModel.EventType}");

                var projector = _resolver.Resolve(projectorType);

                _logger.LogInformation(eventId, $"{projectorType.FullName} was successfully resolved!");

                try
                {
                    _logger.LogInformation(eventId, $"Start processing ... ");

                    ((ImAProjector)projector).Process(@event).Wait();

                    _logger.LogInformation(eventId, $"end of processing, Successful !");

                }
                catch (Exception ex)
                {
                    _logger.LogError(eventId,
                        $"An error occurred in processing event {eventViewModel.EventType} by {projectorType.FullName}, exception {ex.Message}, inner {ex.InnerException}");
                }
                finally
                {

                }
            }
        }

        try
        {
            _quantumDbContext.SaveChanges();
            SaveViewedEvent(eventViewModel, @event);
            SaveCheckPointAsync(@eventViewModel.GlobalCommitPosition, @eventViewModel.GlobalPreparePosition,
                eventViewModel.Version);

            _quantumDbContext.SaveChanges();
        }
        catch (Exception e)
        {
            _quantumDbContext.ChangeTracker.DetectChanges();
        }
        finally
        {

            SaveViewedEvent(eventViewModel, @event);
            SaveCheckPointAsync(@eventViewModel.GlobalCommitPosition, @eventViewModel.GlobalPreparePosition, eventViewModel.Version);

            _quantumDbContext.SaveChanges();
        }
    }

    private void SaveViewedEvent(EventViewModel eventViewModel, IsADomainEvent @event)
    {
        var _deDuplicator = _resolver.Resolve<IDeDuplicator>();
        _deDuplicator.Save(@event.MessageMetadata.Id, eventViewModel.EventType).Wait();
    }

    private bool IsItADuplicateEventWeHaveSeenBefore(string eventId, string eventType)
    {
        var _deDuplicator = _resolver.Resolve<IDeDuplicator>();

        if (!_deDuplicator.IsThisADuplicateEventWeHaveSeenBefore(eventId, eventType).Result) return false;

        _logger.LogInformation(eventId, $"We have seen {eventType} before");

        return true;

    }

    public void LiveProcessingStarted()
    {
    }

    public void SaveCheckPointAsync(ulong commitPosition, ulong preparePosition, int version)
    {
        var _documentStore = _resolver.Resolve<IDocumentStore>();
        _documentStore.StoreCheckpoint(new CheckPoint(Name, commitPosition, preparePosition, version)).Wait();
    }
}