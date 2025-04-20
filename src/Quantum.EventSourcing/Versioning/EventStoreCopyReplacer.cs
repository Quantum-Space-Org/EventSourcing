//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Quantum.Domain;

//namespace Quantum.EventSourcing.Versioning;

//public class EventStoreCopyReplacer : IEventStoreVerioner
//{
//    private readonly IEventStore _eventStore;
//    private IEventTransformerRegistrar _registrar;
//    public EventStoreCopyReplacer(IEventStore eventStore, IEventTransformerRegistrar registrar)
//    {
//        _eventStore = eventStore;
//        _registrar = registrar;
//    }

//    public async Task CopyEventStream(IsAnIdentity from, IsAnIdentity to, bool deleteOldStream = false)
//    {
//        var pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(from);
//        GuardAgainstEmptyOrNotExistStream(pagedEventStreamViewModel, from);

//        await Copy(to, pagedEventStreamViewModel);

//        if (deleteOldStream)
//            await DeleteEventStream(from);
//        else
//            await AppendMovedToEventToTheOfStream(from, to);
//    }

//    public async Task TransformAndCopyEventStream(IsAnIdentity from, IsAnIdentity to, bool deleteOldStream)
//    {
//        var pagedEventStreamViewModel = await _eventStore.LoadEventStreamAsync(from);
//        GuardAgainstEmptyOrNotExistStream(pagedEventStreamViewModel, from);

//        await TransformAndCopy(to, pagedEventStreamViewModel);

//        if (deleteOldStream)
//            await DeleteEventStream(from);
//        else
//            await AppendMovedToEventToTheOfStream(from, to);
//    }



//    private async Task DeleteEventStream(IsAnIdentity from)
//    {
//        await _eventStore.AppendToEventStreamAsync(from, AppendEventDto.Version1(new StreamWasDeletedEvent(from.ToString())));
//    }
//    private async Task AppendMovedToEventToTheOfStream(IsAnIdentity from, IsAnIdentity to)
//    {
//        await _eventStore.AppendToEventStreamAsync(from, AppendEventDto.Version1(new MovedToEvent(from.ToString(), to.ToString())));
//    }
//    private async Task Copy(IsAnIdentity to, PagedEventStreamViewModel pagedEventStreamViewModel)
//    {
//        await _eventStore.AppendToEventStreamAsync(to,
//            pagedEventStreamViewModel.Payloads.Select(AppendEventDto.Version1).ToList());
//    }

//    private async Task TransformAndCopy(IsAnIdentity to, PagedEventStreamViewModel pagedEventStreamViewModel)
//    {
//        List<IsADomainEvent> events = new List<IsADomainEvent>();

//        foreach (var eventViewModel in pagedEventStreamViewModel.Events)
//        {
//            var transformer = _registrar.GetTransformerOf(Type.GetType(eventViewModel.EventType));

//            var methodInfo = transformer.GetType()
//                .GetMethod("Transform");

//            var transformedDomainEvents = methodInfo.Invoke(transformer, new[] { eventViewModel.Payload });

//            var isADomainEvents = (List<IsADomainEvent>)transformedDomainEvents;

//            events.AddRange(isADomainEvents);
//        }

//        await _eventStore.AppendToEventStreamAsync(to, events.Select(AppendEventDto.Version1).ToList());
//    }
//    private void GuardAgainstEmptyOrNotExistStream(PagedEventStreamViewModel pagedEventStreamViewModel, IsAnIdentity from)
//    {
//        if (pagedEventStreamViewModel.Count == 0 || !pagedEventStreamViewModel.Payloads.Any())
//            throw new CopyAnEmptyOrNotExistEventStreamException(from.ToString());

//    }
//}